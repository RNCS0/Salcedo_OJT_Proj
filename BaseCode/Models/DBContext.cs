using BaseCode.Models.Requests;
using BaseCode.Models.Responses;
using BaseCode.Models.Tables;
// Password Hasher
using BCrypt.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
// Email Libraries
using System.Net.Mail;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
// SMS Libraries
using Vonage;
using Vonage.Messaging;
using Vonage.Request;
using Vonage.Users;
using User = BaseCode.Models.Tables.User;
using Product = BaseCode.Models.Tables.Product;

namespace BaseCode.Models
{
    public class DBContext
    {
        public string ConnectionString { get; set; }
        public DBContext(string connStr)
        {
            this.ConnectionString = connStr;            
        }
        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public GenericInsertUpdateResponse InsertUpdateData(GenericInsertUpdateRequest r)
        {
            GenericInsertUpdateResponse resp = new GenericInsertUpdateResponse();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    MySqlTransaction myTrans;
                    myTrans = conn.BeginTransaction();
                    MySqlCommand cmd = new MySqlCommand(r.query, conn);
                    cmd.ExecuteNonQuery();

                    resp.Id = r.isInsert ? int.Parse(cmd.LastInsertedId.ToString()) : -1;
                    myTrans.Commit();
                    conn.Close();
                    resp.isSuccess = true;
                    resp.Message = r.responseMessage;
                }
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = r.errorMessage + ": " + ex.Message;
            }
            return resp;
        }
        public CreateUserResponse CreateUserUsingSqlScript(CreateUserRequest r)
        {
            CreateUserResponse resp = new CreateUserResponse();
            string query = " INSERT INTO USER (USER_FIRST_NAME, USER_LAST_NAME, USER_USERNAME, USER_PASSWORD )";
            query += "VALUES ('" + r.FirstName + "','" + r.LastName + "','" + r.UserName + "','" + r.Password + "')";

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();

            genReq.query = query;
            genReq.isInsert = true;
            genReq.responseMessage = " Successfully created user.";
            genReq.errorMessage = " Unable to create user";

            GenericInsertUpdateResponse genResp = new GenericInsertUpdateResponse();
            genResp = InsertUpdateData(genReq);

            resp.Message = genResp.Message;
            resp.isSuccess = genResp.isSuccess;
            resp.UserId = genResp.Id;

            return resp;
        }

        public CreateUserResponse UpdateUser(CreateUserRequest r)
        {
            CreateUserResponse resp = new CreateUserResponse();

            DateTime theDate = DateTime.Now;
            string crtdt = theDate.ToString("yyyy-MM-dd H:mm:ss");

            string query = "UPDATE USER SET ";
            query += !string.IsNullOrEmpty(r.FirstName) ? " USER_FIRST_NAME = '"+ r.FirstName + "',": "";
            query += !string.IsNullOrEmpty(r.LastName) ? " USER_LAST_NAME = '" + r.LastName  + "'," : "";
            query += !string.IsNullOrEmpty(r.UserName) ? " USER_USERNAME = '" + r.UserName  + "'," : "";
            query += !string.IsNullOrEmpty(r.Password) ? " USER_PASSWORD = '" +  r.Password + "'," : "";
            query = query.TrimEnd(',', ' ');
            query += " WHERE USER_ID = " + r.UserId;


            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();

            genReq.query = query;
            genReq.isInsert = false;
            genReq.responseMessage = " Successfully updated user.";
            genReq.errorMessage = " Unable to update user";

            GenericInsertUpdateResponse genResp = new GenericInsertUpdateResponse();
            genResp = InsertUpdateData(genReq);

            resp.Message = genResp.Message;
            resp.isSuccess = genResp.isSuccess;
            resp.UserId = genResp.Id;

            return resp;
        }

        public GenericGetDataResponse GetData(string query)
        {
            GenericGetDataResponse resp = new GenericGetDataResponse();
            DataTable dt;
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    dt = new DataTable();
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        dt.Load(reader);
                        reader.Close();
                    }
                    conn.Close();
                }
                resp.isSuccess = true;
                resp.Message = "Successfully get data";
                resp.Data = dt;

            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = ex.Message;
            }
            return resp;
        }

        // --------------------------------------------------

        // Functions -----------------------------------------------------------------------------------
        public string GetSettingsValue(string settingKey)
        {
            string query = "SELECT SETTING_VALUE FROM SETTINGS WHERE SETTING_KEY = '" + settingKey + "'";

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                var result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : "";
            }
        }

        public bool CheckSessionActive(string sessionKey, string userType, string expectedStatus = "ACTIVE")
        {
            string checkQuery = "";

            if (userType.ToUpper() == "SELLER")
            {
                checkQuery = "SELECT COUNT(*) FROM SELLER_SESSION WHERE SESSION_KEY = '" + sessionKey.Replace("'", "''") + "' AND SESSION_STATUS = '" + expectedStatus + "'";
            }
            else if (userType.ToUpper() == "BUYER")
            {
                checkQuery = "SELECT COUNT(*) FROM BUYER_SESSION WHERE SESSION_KEY = '" + sessionKey.Replace("'", "''") + "' AND SESSION_STATUS = '" + expectedStatus + "'";
            }
            else
            {
                return false;
            }

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(checkQuery, conn);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        public bool CheckUserExists(string username, string email, string userType)
        {
            string checkQuery = "";
            string activeStatus = GetSettingsValue("ACTIVE");

            if (userType.ToUpper() == "SELLER")
            {
                checkQuery = "SELECT COUNT(*) FROM SELLERS WHERE (SELLER_USERNAME = '" + username.Replace("'", "''") +
                            "' OR SELLER_EMAIL = '" + email.Replace("'", "''") +
                            "') AND SELLER_STATUS = '" + activeStatus + "'";
            }
            else if (userType.ToUpper() == "BUYER")
            {
                checkQuery = "SELECT COUNT(*) FROM BUYERS WHERE (BUYER_USERNAME = '" + username.Replace("'", "''") +
                            "' OR BUYER_EMAIL = '" + email.Replace("'", "''") +
                            "') AND BUYER_STATUS = '" + activeStatus + "'";
            }
            else
            {
                return false;
            }

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(checkQuery, conn);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        public bool CheckUserActive(string username, string userType, string expectedStatus = "ACTIVE")
        {
            string checkQuery = "";

            if (userType.ToUpper() == "SELLER")
            {
                checkQuery = "SELECT COUNT(*) FROM SELLERS WHERE SELLER_USERNAME = '" + username.Replace("'", "''") + "' AND SELLER_STATUS = '" + expectedStatus + "'";
            }
            else if (userType.ToUpper() == "BUYER")
            {
                checkQuery = "SELECT COUNT(*) FROM BUYERS WHERE BUYER_USERNAME = '" + username.Replace("'", "''") + "' AND BUYER_STATUS = '" + expectedStatus + "'";
            }
            else
            {
                return false;
            }

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(checkQuery, conn);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        public bool CheckQuery(string tableName, string condition)
        {
            string query = "SELECT COUNT(*) FROM " + tableName + " WHERE " + condition;

            GenericGetDataResponse checkData = GetData(query);

            if (checkData.Data.Rows.Count > 0)
            {
                int count = Convert.ToInt32(checkData.Data.Rows[0][0]);
                return count > 0;
            }

            return false;
        }

        public (int userId, string password) GetUserIdAndPassword(string username, string userType)
        {
            string query = "";

            if (userType.ToUpper() == "SELLER")
            {
                query = "SELECT SELLER_ID, SELLER_PASSWORD FROM SELLERS WHERE SELLER_USERNAME = '" + username.Replace("'", "''") + "'";
            }
            else if (userType.ToUpper() == "BUYER")
            {
                query = "SELECT BUYER_ID, BUYER_PASSWORD FROM BUYERS WHERE BUYER_USERNAME = '" + username.Replace("'", "''") + "'";
            }
            else
            {
                return (-1, "");
            }

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int userId = reader.GetInt32(0);
                        string password = reader.GetString(1);
                        return (userId, password);
                    }
                }
                conn.Close();
            }

            return (-1, "");
        }

        public int GetUserIdFromSession(string sessionKey, string userType)
        {
            string query = "";

            if (userType.ToUpper() == "SELLER")
            {
                query = "SELECT SELLER_ID FROM SELLER_SESSION WHERE SESSION_KEY = '" + sessionKey.Replace("'", "''") + "'";
            }
            else if (userType.ToUpper() == "BUYER")
            {
                query = "SELECT BUYER_ID FROM BUYER_SESSION WHERE SESSION_KEY = '" + sessionKey.Replace("'", "''") + "'";
            }
            else
            {
                return -1;
            }

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }

        public bool CheckUserActiveByID(int userId, string userType, string expectedStatus = "ACTIVE")
        {
            string checkQuery = "";

            if (userType.ToUpper() == "SELLER")
            {
                checkQuery = "SELECT COUNT(*) FROM SELLERS WHERE SELLER_ID = " + userId + " AND SELLER_STATUS = '" + expectedStatus + "'";
            }
            else if (userType.ToUpper() == "BUYER")
            {
                checkQuery = "SELECT COUNT(*) FROM BUYERS WHERE BUYER_ID = " + userId + " AND BUYER_STATUS = '" + expectedStatus + "'";
            }
            else
            {
                return false;
            }

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(checkQuery, conn);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private void DeactivateExistingOTP(int sellerId)
        {
            string inactiveStatus = GetSettingsValue("INACTIVE");
            string activeStatus = GetSettingsValue("ACTIVE");

            string updateQuery = "UPDATE SELLER_OTP SET OTP_STATUS = '" + inactiveStatus + "' WHERE SELLER_ID = " + sellerId + " AND OTP_STATUS = '" + activeStatus + "'";

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(updateQuery, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }
        private void DeactivateUserSessions(int userId, bool isSeller)
        {
            string inactiveStatus = GetSettingsValue("INACTIVE");
            string activeStatus = GetSettingsValue("ACTIVE");
            DateTime logoutTime = DateTime.Now;
            string logoutDate = logoutTime.ToString("yyyy-MM-dd HH:mm:ss");

            string updateQuery = "";

            if (isSeller)
            {
                updateQuery = "UPDATE SELLER_SESSION SET SESSION_STATUS = '" + inactiveStatus + "', LOGOUT_DATE = '" + logoutDate + "' WHERE SELLER_ID = " + userId + " AND SESSION_STATUS = '" + activeStatus + "'";
            }
            else
            {
                updateQuery = "UPDATE BUYER_SESSION SET SESSION_STATUS = '" + inactiveStatus + "', LOGOUT_DATE = '" + logoutDate + "' WHERE BUYER_ID = " + userId + " AND SESSION_STATUS = '" + activeStatus + "'";
            }

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(updateQuery, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public GenericGetDataResponse GetUserData(int userId, string userType, string statusValue)
        {
            string query = "";

            if (userType.ToUpper() == "SELLER")
            {
                query = "SELECT SELLER_ID, SELLER_FIRST_NAME, SELLER_LAST_NAME, SELLER_USERNAME, " +
                        "SELLER_EMAIL, SELLER_CONTACT_NO, SELLER_STORE_NAME, SELLER_STATUS " +
                        "FROM SELLERS WHERE SELLER_ID = " + userId + " " +
                        "AND SELLER_STATUS = '" + statusValue + "'";
            }
            else if (userType.ToUpper() == "BUYER")
            {
                query = "SELECT BUYER_ID, BUYER_FIRST_NAME, BUYER_LAST_NAME, BUYER_USERNAME, " +
                        "BUYER_EMAIL, BUYER_CONTACT_NO, BUYER_STATUS " +
                        "FROM BUYERS WHERE BUYER_ID = " + userId + " " +
                        "AND BUYER_STATUS = '" + statusValue + "'";
            }
            else
            {
                return new GenericGetDataResponse { isSuccess = false, Message = "Invalid user type", Data = new DataTable() };
            }

            return GetData(query);
        }

        // Functions for Products ---------------------------------------------------------------------------------

        public bool CheckProductExists(int productId, int sellerId, string expectedStatus = null)
        {
            string checkQuery = "SELECT COUNT(*) FROM PRODUCTS WHERE PRODUCT_ID = " + productId +
                                " AND SELLER_ID = " + sellerId;

            if (!string.IsNullOrEmpty(expectedStatus))
            {
                string statusValue = GetSettingsValue(expectedStatus);
                checkQuery += " AND PRODUCT_STATUS = '" + statusValue + "'";
            }

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(checkQuery, conn);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();
                return count > 0;
            }
        }
        public bool CheckProductBelongsToSeller(int productId, int sellerId)
        {
            string checkQuery = "SELECT COUNT(*) FROM PRODUCTS WHERE PRODUCT_ID = " + productId +
                                " AND SELLER_ID = " + sellerId;

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(checkQuery, conn);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();
                return count > 0;
            }
        }
        public int GetProductSellerId(int productId)
        {
            string query = "SELECT SELLER_ID FROM PRODUCTS WHERE PRODUCT_ID = " + productId;

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                var result = cmd.ExecuteScalar();
                conn.Close();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }

        public GenericGetDataResponse SearchProductsQuery(string keyword, string status = "ACTIVE", int? sellerId = null, int? categoryId = null)
        {
            string activeStatus = GetSettingsValue("ACTIVE");
            string soldOutStatus = GetSettingsValue("SOLD_OUT");
            string searchKeyword = "%" + keyword.Replace("'", "''") + "%";

            string query = "SELECT p.*, pc.CATEGORY_NAME, " +
                "s.SELLER_STORE_NAME, s.SELLER_FIRST_NAME, s.SELLER_LAST_NAME, " +
                "CASE " +
                "WHEN p.PRODUCT_STATUS = '" + soldOutStatus + "' THEN 'SOLD OUT' " +
                "WHEN p.PRODUCT_STATUS = '" + activeStatus + "' THEN 'ACTIVE' " +
                "ELSE 'INACTIVE' END as DISPLAY_STATUS " +
                "FROM PRODUCTS p " +
                "LEFT JOIN PRODUCT_CATEGORY pc ON p.CATEGORY_ID = pc.CATEGORY_ID " +
                "LEFT JOIN SELLERS s ON p.SELLER_ID = s.SELLER_ID " +
                "WHERE 1=1" +

                (status == "ACTIVE" ?
                " AND p.PRODUCT_STATUS IN ('" + activeStatus + "', '" + soldOutStatus + "')" :
                (status != "ALL" ? " AND p.PRODUCT_STATUS = '" + status + "'" : "")) +

                (sellerId.HasValue && sellerId.Value > 0 ?
                " AND p.SELLER_ID = " + sellerId.Value : "") +

                (categoryId.HasValue && categoryId.Value > 0 ?
                " AND p.CATEGORY_ID = " + categoryId.Value : "") +

                (!string.IsNullOrEmpty(keyword) ?
                " AND (p.PRODUCT_NAME LIKE '" + searchKeyword + "'" +
                " OR p.PRODUCT_BRAND LIKE '" + searchKeyword + "'" +
                " OR pc.CATEGORY_NAME LIKE '" + searchKeyword + "'" +
                " OR s.SELLER_STORE_NAME LIKE '" + searchKeyword + "')" : "") +

                " ORDER BY p.DATE_ADDED DESC";

            return GetData(query);
        }

        public List<Product> MapDataTableToProductList(DataTable dt)
        {
            List<Product> productList = new List<Product>();

            foreach (DataRow dr in dt.Rows)
            {
                Product p = new Product();
                p.ProductId = int.Parse(dr["PRODUCT_ID"].ToString());
                p.SellerId = int.Parse(dr["SELLER_ID"].ToString());
                p.ProductName = dr["PRODUCT_NAME"].ToString();
                p.ProductDescription = dr["PRODUCT_DESCRIPTION"].ToString();
                p.Price = decimal.Parse(dr["PRODUCT_PRICE"].ToString());
                p.Quantity = int.Parse(dr["PRODUCT_QUANTITY"].ToString());
                p.Category = dr.Table.Columns.Contains("CATEGORY_NAME") ? dr["CATEGORY_NAME"]?.ToString() ?? "" : "";
                p.Brand = dr["PRODUCT_BRAND"]?.ToString() ?? "";
                p.StoreName = dr.Table.Columns.Contains("SELLER_STORE_NAME") ? dr["SELLER_STORE_NAME"]?.ToString() ?? "" : "";
                p.SellerName = dr.Table.Columns.Contains("SELLER_FIRST_NAME") && dr.Table.Columns.Contains("SELLER_LAST_NAME")
                    ? dr["SELLER_FIRST_NAME"] + " " + dr["SELLER_LAST_NAME"]
                    : "";

                string productStatus = dr["PRODUCT_STATUS"].ToString();
                string activeStatus = GetSettingsValue("ACTIVE");
                string soldOutStatus = GetSettingsValue("SOLD_OUT");

                if (productStatus == activeStatus)
                {
                    p.Status = "ACTIVE";
                }
                else if (productStatus == soldOutStatus)
                {
                    p.Status = "SOLD OUT";
                }
                else
                {
                    p.Status = "INACTIVE";
                }

                p.DateAdded = DateTime.Parse(dr["DATE_ADDED"].ToString());

                productList.Add(p);
            }

            return productList;
        }

        private void UpdateProductStatusBasedOnQuantity(int productId)
        {
            string activeStatus = GetSettingsValue("ACTIVE");
            string soldOutStatus = GetSettingsValue("SOLD_OUT");

            string query = "UPDATE PRODUCTS SET PRODUCT_STATUS = " +
                           "CASE " +
                           "    WHEN PRODUCT_QUANTITY > 0 THEN '" + activeStatus + "' " +
                           "    ELSE '" + soldOutStatus + "' " +
                           "END " +
                           "WHERE PRODUCT_ID = " + productId;

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        // Functions for Voucher ---------------------------------------------------------------
        private bool CheckVoucherCodeExists(string voucherCode)
        {
            string query = "SELECT COUNT(*) FROM VOUCHERS WHERE VOUCHER_CODE = '" + voucherCode.Replace("'", "''") + "'";

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();
                return count > 0;
            }
        }

        private bool CheckVoucherBelongsToSeller(int voucherId, int sellerId)
        {
            string query = "SELECT COUNT(*) FROM VOUCHERS WHERE VOUCHER_ID = " + voucherId + " AND SELLER_ID = " + sellerId;

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();
                return count > 0;
            }
        }

        private int GetVoucherIdFromCode(string voucherCode)
        {
            string query = "SELECT VOUCHER_ID FROM VOUCHERS WHERE VOUCHER_CODE = '" + voucherCode.Replace("'", "''") + "'";

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                object result = cmd.ExecuteScalar();
                conn.Close();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
        private decimal GetVoucherDiscountAmount(int voucherId)
        {
            string query = "SELECT DISCOUNT_AMOUNT FROM VOUCHERS WHERE VOUCHER_ID = " + voucherId;

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                object result = cmd.ExecuteScalar();
                conn.Close();
                return result != null ? Convert.ToDecimal(result) : 0;
            }
        }

        private bool CheckVoucherActive(string voucherCode)
        {
            string activeStatus = GetSettingsValue("ACTIVE");
            DateTime now = DateTime.Now;
            string currentDate = now.ToString("yyyy-MM-dd HH:mm:ss");

            string query = "SELECT COUNT(*) FROM VOUCHERS WHERE VOUCHER_CODE = '" + voucherCode.Replace("'", "''") + "' " +
                           "AND VOUCHER_STATUS = '" + activeStatus + "' " +
                           "AND VALID_FROM <= '" + currentDate + "' " +
                           "AND VALID_TO >= '" + currentDate + "'";

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();
                return count > 0;
            }
        }

        private bool CheckVoucherMaxUsesReached(int voucherId)
        {
            string query = "SELECT USED_COUNT, MAX_USES FROM VOUCHERS WHERE VOUCHER_ID = " + voucherId;

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    int usedCount = reader.GetInt32(0);
                    int maxUses = reader.GetInt32(1);
                    reader.Close();
                    conn.Close();
                    return usedCount >= maxUses;
                }
                reader.Close();
                conn.Close();
                return true;
            }
        }
        private void UpdateVoucherUsedCount(int voucherId)
        {
            string query = "UPDATE VOUCHERS SET USED_COUNT = USED_COUNT + 1, DATE_UPDATED = NOW() WHERE VOUCHER_ID = " + voucherId;

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        // -------------------------------------------------------------------------------------





        // Seller Registration ------------------------------------------------------------
        public RegisterSellerResponse RegisterSeller(RegisterSellerRequest r)
        {
            RegisterSellerResponse resp = new RegisterSellerResponse();
            string activeStatus = GetSettingsValue("ACTIVE");
            DateTime currentDate = DateTime.Now;
            string dateAdded = currentDate.ToString("yyyy-MM-dd HH:mm:ss");

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();

                string tempPassword = Guid.NewGuid().ToString().Substring(0, 8);
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(tempPassword);

                string otp = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8).ToUpper();
                resp.OTP = otp;

                string sellerQuery = "INSERT INTO SELLERS (SELLER_FIRST_NAME, SELLER_LAST_NAME, SELLER_USERNAME, SELLER_PASSWORD, " + "SELLER_EMAIL, SELLER_CONTACT_NO, SELLER_STORE_NAME, SELLER_STATUS, DATE_ADDED) " + "VALUES ('" + r.FirstName + "', '" + r.LastName + "', '" + r.UserName + "', '" + hashedPassword + "', '" + r.Email + "', '" + r.ContactNo + "', '" + r.StoreName + "', '" + activeStatus + "', '" + dateAdded + "')";

                MySqlCommand cmd = new MySqlCommand(sellerQuery, conn);
                cmd.ExecuteNonQuery();

                int sellerId = int.Parse(cmd.LastInsertedId.ToString());
                resp.UserId = sellerId;
                resp.TemporaryPassword = tempPassword;

                DeactivateExistingOTP(sellerId);

                string activeOTPStatus = GetSettingsValue("ACTIVE");
                string otpQuery = "INSERT INTO SELLER_OTP (SELLER_ID, OTP, OTP_EXPIRY_DATE, OTP_STATUS) " + "VALUES ('" + sellerId + "', '" + otp + "', DATE_ADD(NOW(), INTERVAL 10 MINUTE), '" + activeOTPStatus + "')";

                MySqlCommand cmd2 = new MySqlCommand(otpQuery, conn);
                cmd2.ExecuteNonQuery();

                SendEmailRegistration(r.Email, r.FirstName, r.UserName, tempPassword, otp);
                resp.isSuccess = true;
                resp.Message = "Seller registration successful! Temporary password and OTP sent via Email. Please verify your email.";

                conn.Close();
            }

            return resp;
        }

        // Buyer Registration -------------------------------------------------------------
        public RegisterBuyerResponse RegisterBuyer(RegisterBuyerRequest r)
        {
            RegisterBuyerResponse resp = new RegisterBuyerResponse();
            string activeStatus = GetSettingsValue("ACTIVE");
            DateTime currentDate = DateTime.Now;
            string dateAdded = currentDate.ToString("yyyy-MM-dd HH:mm:ss");

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();

                string tempPassword = Guid.NewGuid().ToString().Substring(0, 8);
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(tempPassword);

                string buyerQuery = "INSERT INTO BUYERS (BUYER_FIRST_NAME, BUYER_LAST_NAME, BUYER_USERNAME, BUYER_PASSWORD, " + "BUYER_EMAIL, BUYER_CONTACT_NO, BUYER_STATUS, DATE_ADDED) " + "VALUES ('" + r.FirstName + "', '" + r.LastName + "', '" + r.UserName + "', '" + hashedPassword + "', '" + r.Email + "', '" + r.ContactNo + "', '" + activeStatus + "', '" + dateAdded + "')";

                MySqlCommand cmd = new MySqlCommand(buyerQuery, conn);
                cmd.ExecuteNonQuery();

                int buyerId = int.Parse(cmd.LastInsertedId.ToString());
                resp.UserId = buyerId;
                resp.TemporaryPassword = tempPassword;

                string contactPreference = r.PassPreference.ToLower();

                if (contactPreference == "sms")
                {
                    string smsMessage = SendPhoneRegistration(r.ContactNo, r.FirstName, r.UserName, tempPassword);
                    resp.isSuccess = true;
                    resp.Message = "Buyer registration successful! Temporary password sent via SMS.";
                }
                else
                {
                    SendEmailRegistration(r.Email, r.FirstName, r.UserName, tempPassword);
                    resp.isSuccess = true;
                    resp.Message = "Buyer registration successful! Temporary password sent via Email.";
                }

                conn.Close();
            }

            return resp;
        }

        // --------------------------------------------------------------------------------

        // Email Registration ------------------------------------------------------
        private void SendEmailRegistration(string toEmail, string firstName, string username, string tempPassword, string otp = null, bool isNewOTP = false)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("neilsalcedo29@gmail.com", "Registration");
                    mail.To.Add(toEmail);

                    if (isNewOTP)
                    {
                        mail.Subject = "New OTP Request";
                    }
                    else
                    {
                        mail.Subject = "Registration";
                    }

                    if (!string.IsNullOrEmpty(otp))
                    {
                        if (isNewOTP)
                        {
                            // NEW OTP EMAIL
                            mail.Body = $"Hello {firstName},<br><br>" +
                                       $"You have requested a new OTP for your account.<br>" +
                                       $"Username: {username}<br><br>" +
                                       $"<strong>Your new OTP for verification: {otp}</strong><br><br>" +
                                       $"This OTP will expire in 10 minutes.<br>" +
                                       $"Please use this OTP to verify your email address.<br><br>" +
                                       $"If you did not request this OTP, please contact support immediately.<br><br>" +
                                       $"Thank you!";
                        }
                        else
                        {
                            // ORIGINAL SELLER REGISTRATION EMAIL WITH OTP
                            mail.Body = $"Hello {firstName},<br><br>" +
                                       $"Your account has been created successfully.<br>" +
                                       $"Username: {username}<br>" +
                                       $"Temporary Password: {tempPassword}<br><br>" +
                                       $"<strong>OTP for verification: {otp}</strong><br><br>" +
                                       $"Please login and verify your email using this OTP within 10 minutes.<br>" +
                                       $"After verification, please change your password immediately.<br><br>" +
                                       $"Thank you!";
                        }
                    }
                    else
                    {
                        // BUYER EMAIL WITHOUT OTP
                        mail.Body = $"Hello {firstName},<br><br>" +
                                   $"Your account has been created successfully.<br>" +
                                   $"Username: {username}<br>" +
                                   $"Temporary Password: {tempPassword}<br><br>" +
                                   $"Please login and change your password immediately.<br><br>" +
                                   $"Thank you!";
                    }

                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("neilsalcedo29@gmail.com", "fnjf zcnd wnkg naua");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
        }

        // SMS Registration -------------------------------------------------------
        private string SendPhoneRegistration(string toNumber, string firstName, string username, string tempPassword)
        {
            try
            {
                var credentials = Credentials.FromApiKeyAndSecret(
                    "b0b82442",
                    "x2Q3TbbYM2C47fvm"
                );

                var VonageClient = new VonageClient(credentials);

                string formattedNumber = toNumber.Replace("+", "");
                if (formattedNumber.StartsWith("639") && formattedNumber.Length == 12)
                {
                    
                }
                else if (formattedNumber.StartsWith("09") && formattedNumber.Length == 11)
                {
                    formattedNumber = "63" + formattedNumber.Substring(1);
                }
                else
                {
                    return "Error: Invalid phone number format: " + toNumber;
                }

                string smsBody = "Hello " + firstName + ", this is your temporary password for " + username + ": " + tempPassword;

                var response = VonageClient.SmsClient.SendAnSmsAsync(new Vonage.Messaging.SendSmsRequest()
                {
                    To = formattedNumber,
                    From = "Registration",
                    Text = smsBody,
                }).Result;

                if (response.Messages[0].Status == "0")
                {
                    return "Success";
                }
                else
                {
                    return "SMS failed ";
                }
            }
            catch (Exception ex)
            {
                return "SMS sending failed";
            }
        }
        // ----------------------------------------------------------------------

        // OTP VERIFICATION -----------------------------------------------------

        public OTPVerificationResponse VerifyOTP(OTPVerificationRequest r)
        {
            OTPVerificationResponse resp = new OTPVerificationResponse();

            string activeStatus = GetSettingsValue("ACTIVE");
            string usedStatus = GetSettingsValue("USED");

            string validCondition = "SELLER_ID = " + r.UserId + " AND OTP = '" + r.OTP + "' AND OTP_STATUS = '" + activeStatus + "' AND OTP_EXPIRY_DATE > NOW()";
            bool hasValidOTP = CheckQuery("SELLER_OTP", validCondition);

            if (hasValidOTP)
            {
                string updateQuery = "UPDATE SELLER_OTP SET OTP_STATUS = '" + usedStatus + "' WHERE SELLER_ID = " + r.UserId + " AND OTP = '" + r.OTP + "'";

                GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
                genReq.query = updateQuery;
                GenericInsertUpdateResponse genResp = InsertUpdateData(genReq);

                if (genResp.isSuccess)
                {
                    resp.isSuccess = true;
                    resp.Message = "OTP verified successfully! Your email is now verified.";
                }
                else
                {
                    resp.isSuccess = false;
                    resp.Message = "Failed to verify OTP.";
                }
            }
            else
            {
                resp.isSuccess = false;
                resp.Message = "Invalid or expired OTP.";
            }

            return resp;
        }

        public RequestNewOTPResponse RequestNewOTP(RequestNewOTPRequest r)
        {
            RequestNewOTPResponse resp = new RequestNewOTPResponse();
            string activeStatus = GetSettingsValue("ACTIVE");
            string getSellerQuery = "SELECT SELLER_ID, SELLER_EMAIL, SELLER_FIRST_NAME, SELLER_USERNAME FROM SELLERS WHERE SELLER_EMAIL = '" + r.Email.Replace("'", "''") + "' AND SELLER_STATUS = '" + activeStatus + "'";

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();

                MySqlCommand sellerCmd = new MySqlCommand(getSellerQuery, conn);
                using (MySqlDataReader reader = sellerCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int userId = reader.GetInt32(0);
                        string sellerEmail = reader.GetString(1);
                        string sellerFirstName = reader.GetString(2);
                        string sellerUsername = reader.GetString(3);

                        reader.Close();
                        DeactivateExistingOTP(userId);

                        // Generate new OTP
                        string newOTP = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8).ToUpper();
                        string otpQuery = "INSERT INTO SELLER_OTP (SELLER_ID, OTP, OTP_EXPIRY_DATE, OTP_STATUS) " +
                                         "VALUES ('" + userId + "', '" + newOTP + "', DATE_ADD(NOW(), INTERVAL 10 MINUTE), '" + activeStatus + "')";

                        MySqlCommand otpCmd = new MySqlCommand(otpQuery, conn);
                        otpCmd.ExecuteNonQuery();

                        // Send email
                        SendEmailRegistration(sellerEmail, sellerFirstName, sellerUsername, "", newOTP, true);

                        resp.isSuccess = true;
                        resp.Message = "New OTP sent to your email";
                        resp.NewOTP = newOTP;
                        resp.UserId = userId;
                    }
                    else
                    {
                        resp.isSuccess = false;
                        resp.Message = "Seller not found or inactive";
                    }
                }
                conn.Close();
            }
            return resp;
        }

        //-----------------------------------------------------------------------
        // Login Seller ---------------------------------------------------------
        public LoginResponse LoginSeller(LoginRequest r)
        {
            LoginResponse resp = new LoginResponse();
            var (userId, databasePassword) = GetUserIdAndPassword(r.UserName, "SELLER");

            if (userId == -1)
            {
                resp.isSuccess = false;
                resp.Message = "Cannot retrieve seller credentials";
                return resp;
            }
            string otpCondition = "SELLER_ID = " + userId + " AND OTP_STATUS = '" + GetSettingsValue("USED") + "'";
            bool hasVerifiedOTP = CheckQuery("SELLER_OTP", otpCondition);

            if (!hasVerifiedOTP)
            {
                resp.isSuccess = false;
                resp.Message = "Cannot login! Please verify your email with OTP first";
                return resp;
            }
            if (!BCrypt.Net.BCrypt.Verify(r.Password, databasePassword))
            {
                resp.isSuccess = false;
                resp.Message = "Cannot Login! Credentials do not match";
                return resp;
            }

            DeactivateUserSessions(userId, true);
            string sessionKey = Guid.NewGuid().ToString();
            string activeSessionStatus = GetSettingsValue("ACTIVE");

            string sessionQuery = "INSERT INTO SELLER_SESSION (SESSION_KEY, SELLER_ID, SESSION_STATUS) " + "VALUES ('" + sessionKey + "', " + userId + ", '" + activeSessionStatus + "')";

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = sessionQuery;
            GenericInsertUpdateResponse genResp = InsertUpdateData(genReq);

            resp.isSuccess = true;
            resp.Message = "Welcome Seller! Login Successful";
            resp.UserId = userId;
            resp.SessionKey = sessionKey;
            resp.UserType = GetSettingsValue("SELLER");

            return resp;
        }

        // Login Buyer ---------------------------------------------------------------
        public LoginResponse LoginBuyer(LoginRequest r)
        {
            LoginResponse resp = new LoginResponse();
            var (userId, databasePassword) = GetUserIdAndPassword(r.UserName, "BUYER");

            if (userId == -1)
            {
                resp.isSuccess = false;
                resp.Message = "Cannot retrieve buyer credentials";
                return resp;
            }

            if (!BCrypt.Net.BCrypt.Verify(r.Password, databasePassword))
            {
                resp.isSuccess = false;
                resp.Message = "Cannot Login! Credentials do not match";
                return resp;
            }
            DeactivateUserSessions(userId, false);

            string sessionKey = Guid.NewGuid().ToString();
            string activeSessionStatus = GetSettingsValue("ACTIVE");
            string sessionQuery = "INSERT INTO BUYER_SESSION (SESSION_KEY, BUYER_ID, SESSION_STATUS) " + "VALUES ('" + sessionKey + "', " + userId + ", '" + activeSessionStatus + "')";

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = sessionQuery;
            GenericInsertUpdateResponse genResp = InsertUpdateData(genReq);

            resp.isSuccess = true;
            resp.Message = "Welcome Buyer! Login Successful";
            resp.UserId = userId;
            resp.SessionKey = sessionKey;
            resp.UserType = GetSettingsValue("BUYER");

            return resp;
        }
        // ---------------------------------------------------------------------------

        // Logout Seller -------------------------------------------------------------
        public LogoutResponse LogoutSeller(LogoutRequest r)
        {
            LogoutResponse resp = new LogoutResponse();
            DateTime logoutTime = DateTime.Now;
            string logoutDate = logoutTime.ToString("yyyy-MM-dd HH:mm:ss");

            int userId = GetUserIdFromSession(r.SessionKey, "SELLER");

            DeactivateUserSessions(userId, true);

            resp.isSuccess = true;
            resp.Message = "Seller logout successful at " + logoutDate;
            resp.LogoutDate = logoutDate;

            return resp;
        }

        // Logout Buyer -------------------------------------------------------------
        public LogoutResponse LogoutBuyer(LogoutRequest r)
        {
            LogoutResponse resp = new LogoutResponse();
            DateTime logoutTime = DateTime.Now;
            string logoutDate = logoutTime.ToString("yyyy-MM-dd HH:mm:ss");

            int userId = GetUserIdFromSession(r.SessionKey, "BUYER");

            DeactivateUserSessions(userId, false);

            resp.isSuccess = true;
            resp.Message = "Buyer logout successful at " + logoutDate;
            resp.LogoutDate = logoutDate;

            return resp;
        }
        // ---------------------------------------------------------------------------

        // Change Password for Seller -------------------------------------
        public ChangePasswordResponse ChangePasswordSeller(ChangePasswordRequest r)
        {
            ChangePasswordResponse resp = new ChangePasswordResponse();
            string activeStatus = GetSettingsValue("ACTIVE");

            string username = "";
            string getUserQuery = "SELECT SELLER_USERNAME FROM SELLERS WHERE SELLER_ID = " + r.UserId + " AND SELLER_STATUS = '" + activeStatus + "'";

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand checkCmd = new MySqlCommand(getUserQuery, conn);
                var usernameResult = checkCmd.ExecuteScalar();
                username = usernameResult.ToString();
                conn.Close();
            }

            var (userId, databasePassword) = GetUserIdAndPassword(username, "SELLER");
            bool passwordCorrect = BCrypt.Net.BCrypt.Verify(r.CurrentPassword, databasePassword);

            if (!passwordCorrect)
            {
                resp.isSuccess = false;
                resp.Message = "Current password is incorrect";
                return resp;
            }
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(r.NewPassword);
            string updateQuery = "UPDATE SELLERS SET SELLER_PASSWORD = '" + hashedPassword + "' WHERE SELLER_ID = " + r.UserId;

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn);
                int rowsUpdated = updateCmd.ExecuteNonQuery();

                if (rowsUpdated > 0)
                {
                    resp.isSuccess = true;
                    resp.Message = "Successfully changed seller password";
                }
                else
                {
                    resp.isSuccess = false;
                    resp.Message = "Cannot change seller password";
                }
                conn.Close();
            }

            return resp;
        }

        // Change Password for Buyer -------------------------------------
        public ChangePasswordResponse ChangePasswordBuyer(ChangePasswordRequest r)
        {
            ChangePasswordResponse resp = new ChangePasswordResponse();
            string activeStatus = GetSettingsValue("ACTIVE");

            string username = "";
            string getUserQuery = "SELECT BUYER_USERNAME FROM BUYERS WHERE BUYER_ID = " + r.UserId + " AND BUYER_STATUS = '" + activeStatus + "'";

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand checkCmd = new MySqlCommand(getUserQuery, conn);
                var usernameResult = checkCmd.ExecuteScalar();
                username = usernameResult.ToString();
                conn.Close();
            }

            var (userId, databasePassword) = GetUserIdAndPassword(username, "BUYER");
            bool passwordCorrect = BCrypt.Net.BCrypt.Verify(r.CurrentPassword, databasePassword);

            if (!passwordCorrect)
            {
                resp.isSuccess = false;
                resp.Message = "Current password is incorrect";
                return resp;
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(r.NewPassword);
            string updateQuery = "UPDATE BUYERS SET BUYER_PASSWORD = '" + hashedPassword + "' WHERE BUYER_ID = " + r.UserId;

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn);
                int rowsUpdated = updateCmd.ExecuteNonQuery();

                if (rowsUpdated > 0)
                {
                    resp.isSuccess = true;
                    resp.Message = "Successfully changed buyer password";
                }
                else
                {
                    resp.isSuccess = false;
                    resp.Message = "Cannot change buyer password";
                }
                conn.Close();
            }

            return resp;
        }
        // --------------------------------------------------------------------------------------------

        // Forgot Password for Seller ----------------------------------------------------------------------------
        public ForgotPasswordResponse ForgotPasswordSeller(ForgotPasswordRequest r, GenericGetDataResponse getUserData)
        {
            ForgotPasswordResponse resp = new ForgotPasswordResponse();

            int userId = Convert.ToInt32(getUserData.Data.Rows[0]["SELLER_ID"]);
            string firstName = getUserData.Data.Rows[0]["SELLER_FIRST_NAME"].ToString();
            string userName = getUserData.Data.Rows[0]["SELLER_USERNAME"].ToString();
            string userEmail = getUserData.Data.Rows[0]["SELLER_EMAIL"].ToString();

            string tempPassword = Guid.NewGuid().ToString().Substring(0, 8);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(tempPassword);

            string updateQuery = "UPDATE SELLERS SET SELLER_PASSWORD = '" + hashedPassword + "' WHERE SELLER_ID = " + userId;

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = updateQuery;
            genReq.isInsert = false;
            genReq.responseMessage = "Seller password reset successful";
            genReq.errorMessage = "Unable to reset seller password";

            GenericInsertUpdateResponse genResp = InsertUpdateData(genReq);

            if (genResp.isSuccess)
            {
                SendForgotPasswordEmail(userEmail, firstName, userName, tempPassword);

                resp.isSuccess = true;
                resp.Message = "Temporary password has been sent to your email";
                resp.TemporaryPassword = tempPassword;
                resp.UserId = userId;
                return resp;
            }
            else
            {
                resp.isSuccess = false;
                resp.Message = "Failed to reset seller password: " + genResp.Message;
                return resp;
            }
        }

        // Forgot Password for Buyer ----------------------------------------------------------------------------
        public ForgotPasswordResponse ForgotPasswordBuyer(ForgotPasswordRequest r, GenericGetDataResponse getUserData)
        {
            ForgotPasswordResponse resp = new ForgotPasswordResponse();
            int userId = Convert.ToInt32(getUserData.Data.Rows[0]["BUYER_ID"]);
            string firstName = getUserData.Data.Rows[0]["BUYER_FIRST_NAME"].ToString();
            string userName = getUserData.Data.Rows[0]["BUYER_USERNAME"].ToString();
            string userEmail = getUserData.Data.Rows[0]["BUYER_EMAIL"].ToString();
            string contactNo = getUserData.Data.Rows[0]["BUYER_CONTACT_NO"].ToString();

            string tempPassword = Guid.NewGuid().ToString().Substring(0, 8);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(tempPassword);


            string updateQuery = "UPDATE BUYERS SET BUYER_PASSWORD = '" + hashedPassword + "' WHERE BUYER_ID = " + userId;

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = updateQuery;
            genReq.isInsert = false;
            genReq.responseMessage = "Buyer password reset successful";
            genReq.errorMessage = "Unable to reset buyer password";

            GenericInsertUpdateResponse genResp = InsertUpdateData(genReq);

            if (genResp.isSuccess)
            {
                SendForgotPasswordEmail(userEmail, firstName, userName, tempPassword);

                resp.isSuccess = true;
                resp.Message = "Temporary password has been sent to your email";
                resp.TemporaryPassword = tempPassword;
                resp.UserId = userId;
                return resp;
            }
            else
            {
                resp.isSuccess = false;
                resp.Message = "Failed to reset buyer password: " + genResp.Message;
                return resp;
            }
        }
        // --------------------------------------------------------------------------------------------

        // Forgot Password Email ----------------------------------------------------------------------

        private void SendForgotPasswordEmail(string toEmail, string firstName, string username, string tempPassword, string otp = null)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("neilsalcedo29@gmail.com", "Registration");
                    mail.To.Add(toEmail);
                    mail.Subject = "Password Reset";

                    mail.Body = $"Hello {firstName},<br><br>" +
                                $"You have requested to reset your password.<br>" +
                                $"Username: {username}<br>" +
                                $"Your new temporary password: <strong>{tempPassword}</strong><br><br>" +
                                $"Please login using this temporary password and change your password immediately.<br><br>" +
                                $"If you did not request this password reset, please contact support immediately.<br><br>" +
                                $"Thank you!";

                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("neilsalcedo29@gmail.com", "fnjf zcnd wnkg naua");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
        }

        // --------------------------------------------------------------------------------------------

        // Delete Seller ------------------------------------------------------------------------------
        public DeleteAccountResponse DeleteSeller(DeleteAccountRequest r)
        {
            DeleteAccountResponse resp = new DeleteAccountResponse();
            string inactiveStatus = GetSettingsValue("INACTIVE");
            DateTime deleteTime = DateTime.Now;
            string deleteDate = deleteTime.ToString("yyyy-MM-dd HH:mm:ss");

            string updateQuery = "UPDATE SELLERS SET SELLER_STATUS = '" + inactiveStatus + "', DATE_UPDATED = '" + deleteDate + "' WHERE SELLER_ID = " + r.UserId;

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(updateQuery, conn);
                int rowsUpdated = cmd.ExecuteNonQuery();

                if (rowsUpdated > 0)
                {
                    DeactivateUserSessions(r.UserId, true);
                    string productQuery = "UPDATE PRODUCTS SET PRODUCT_STATUS = '" + inactiveStatus + "', DATE_UPDATED = '" + deleteDate + "' WHERE SELLER_ID = " + r.UserId;
                    MySqlCommand productCmd = new MySqlCommand(productQuery, conn);
                    productCmd.ExecuteNonQuery();

                    resp.isSuccess = true;
                    resp.Message = "Seller account successfully deactivated at " + deleteDate;
                    resp.DeactivationDate = deleteDate;
                }
                else
                {
                    resp.isSuccess = false;
                    resp.Message = "Failed to deactivate seller account";
                }
                conn.Close();
            }

            return resp;
        }

        // Delete Buyer  ----------------------------------------------------------------
        public DeleteAccountResponse DeleteBuyer(DeleteAccountRequest r)
        {
            DeleteAccountResponse resp = new DeleteAccountResponse();
            string inactiveStatus = GetSettingsValue("INACTIVE");
            DateTime deleteTime = DateTime.Now;
            string deleteDate = deleteTime.ToString("yyyy-MM-dd HH:mm:ss");
            string updateQuery = "UPDATE BUYERS SET BUYER_STATUS = '" + inactiveStatus + "', DATE_UPDATED = '" + deleteDate + "' WHERE BUYER_ID = " + r.UserId;

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(updateQuery, conn);
                int rowsUpdated = cmd.ExecuteNonQuery();

                if (rowsUpdated > 0)
                {

                    DeactivateUserSessions(r.UserId, false);

                    resp.isSuccess = true;
                    resp.Message = "Buyer account successfully deactivated at " + deleteDate;
                    resp.DeactivationDate = deleteDate;
                }
                else
                {
                    resp.isSuccess = false;
                    resp.Message = "Failed to deactivate buyer account";
                }
                conn.Close();
            }

            return resp;
        }

        // Get Seller Profile  -----------------------------------------------------------------
        public GetSellerProfileResponse GetSellerProfile(GetSellerProfileRequest r)
        {
            GetSellerProfileResponse resp = new GetSellerProfileResponse();
            string activeStatus = GetSettingsValue("ACTIVE");

            // Add DATE_ADDED back to your query
            string query = "SELECT SELLER_ID, SELLER_FIRST_NAME, SELLER_LAST_NAME, SELLER_USERNAME, " +
                           "SELLER_EMAIL, SELLER_CONTACT_NO, SELLER_STORE_NAME, SELLER_STATUS, DATE_ADDED " + // Added DATE_ADDED back
                           "FROM SELLERS WHERE SELLER_ID = " + r.SellerId + " " +
                           "AND SELLER_STATUS = '" + activeStatus + "'";

            GenericGetDataResponse getData = GetData(query);

            if (getData.isSuccess && getData.Data.Rows.Count > 0)
            {
                DataRow dr = getData.Data.Rows[0];
                resp.SellerId = Convert.ToInt32(dr["SELLER_ID"]);
                resp.FirstName = dr["SELLER_FIRST_NAME"]?.ToString() ?? "";
                resp.LastName = dr["SELLER_LAST_NAME"]?.ToString() ?? "";
                resp.UserName = dr["SELLER_USERNAME"]?.ToString() ?? "";
                resp.Email = dr["SELLER_EMAIL"]?.ToString() ?? "";
                resp.ContactNo = dr["SELLER_CONTACT_NO"]?.ToString() ?? "";
                resp.StoreName = dr["SELLER_STORE_NAME"]?.ToString() ?? "";
                resp.Status = dr["SELLER_STATUS"]?.ToString() ?? "";

                if (dr["DATE_ADDED"] != DBNull.Value)
                {
                    resp.DateRegistered = Convert.ToDateTime(dr["DATE_ADDED"]);
                }

                try
                {
                    using (MySqlConnection conn = GetConnection())
                    {
                        conn.Open();
                        string productQuery = "SELECT COUNT(*) FROM PRODUCTS WHERE SELLER_ID = @sellerId AND PRODUCT_STATUS = @status";
                        MySqlCommand cmd = new MySqlCommand(productQuery, conn);
                        cmd.Parameters.AddWithValue("@sellerId", r.SellerId);
                        cmd.Parameters.AddWithValue("@status", activeStatus);
                        resp.TotalProducts = Convert.ToInt32(cmd.ExecuteScalar());
                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Product count error: {ex.Message}");
                    resp.TotalProducts = 0;
                }

                resp.isSuccess = true;
                resp.Message = "Seller profile retrieved successfully";
            }
            else
            {
                resp.isSuccess = false;
                resp.Message = getData.isSuccess ? "Seller not found or inactive" : "Database error: " + getData.Message;
            }

            return resp;
        }

        // -------------------------------------------------------------------------------------

        // Get Buyer Profile -------------------------------------------------------------------
        public GetBuyerProfileResponse GetBuyerProfile(GetBuyerProfileRequest r)
        {
            GetBuyerProfileResponse resp = new GetBuyerProfileResponse();
            string activeStatus = GetSettingsValue("ACTIVE");

            string query = "SELECT BUYER_ID, BUYER_FIRST_NAME, BUYER_LAST_NAME, BUYER_USERNAME, " +
                           "BUYER_EMAIL, BUYER_CONTACT_NO, BUYER_STATUS, DATE_ADDED " +
                           "FROM BUYERS WHERE BUYER_ID = " + r.BuyerId + " " +
                           "AND BUYER_STATUS = '" + activeStatus + "'";

            GenericGetDataResponse getData = GetData(query);

            if (getData.isSuccess && getData.Data.Rows.Count > 0)
            {
                DataRow dr = getData.Data.Rows[0];
                resp.BuyerId = Convert.ToInt32(dr["BUYER_ID"]);
                resp.FirstName = dr["BUYER_FIRST_NAME"]?.ToString() ?? "";
                resp.LastName = dr["BUYER_LAST_NAME"]?.ToString() ?? "";
                resp.UserName = dr["BUYER_USERNAME"]?.ToString() ?? "";
                resp.Email = dr["BUYER_EMAIL"]?.ToString() ?? "";
                resp.ContactNo = dr["BUYER_CONTACT_NO"]?.ToString() ?? "";
                resp.Status = dr["BUYER_STATUS"]?.ToString() ?? "";

                if (dr["DATE_ADDED"] != DBNull.Value)
                {
                    resp.DateRegistered = Convert.ToDateTime(dr["DATE_ADDED"]);
                }

                try
                {
                    using (MySqlConnection conn = GetConnection())
                    {
                        conn.Open();
                        string orderQuery = "SELECT COUNT(*) FROM ORDERS WHERE BUYER_ID = @buyerId";
                        MySqlCommand cmd = new MySqlCommand(orderQuery, conn);
                        cmd.Parameters.AddWithValue("@buyerId", r.BuyerId);
                        resp.TotalOrders = Convert.ToInt32(cmd.ExecuteScalar());
                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Order count error: {ex.Message}");
                    resp.TotalOrders = 0; 
                }
                resp.isSuccess = true;
                resp.Message = "Buyer profile retrieved successfully";
            }
            else
            {
                resp.isSuccess = false;
                resp.Message = getData.isSuccess ? "Buyer not found or inactive" : "Database error: " + getData.Message;
            }

            return resp;
        }

        // -------------------------------------------------------------------------------------

        // Update Seller Profile ----------------------------------------------------------------------

        public UpdateSellerProfileResponse UpdateSellerProfile(UpdateSellerProfileRequest r)
        {
            UpdateSellerProfileResponse resp = new UpdateSellerProfileResponse();
            DateTime theDate = DateTime.Now;
            string updatedDt = theDate.ToString("yyyy-MM-dd H:mm:ss");

            string query = "UPDATE SELLERS SET ";
            List<string> updateFields = new List<string>();

            if (!string.IsNullOrEmpty(r.FirstName))
                updateFields.Add("SELLER_FIRST_NAME = '" + r.FirstName.Replace("'", "''") + "'");

            if (!string.IsNullOrEmpty(r.LastName))
                updateFields.Add("SELLER_LAST_NAME = '" + r.LastName.Replace("'", "''") + "'");

            if (!string.IsNullOrEmpty(r.StoreName))
                updateFields.Add("SELLER_STORE_NAME = '" + r.StoreName.Replace("'", "''") + "'");

            if (!string.IsNullOrEmpty(r.UserName))
                updateFields.Add("SELLER_USERNAME = '" + r.UserName.Replace("'", "''") + "'");

            updateFields.Add("DATE_UPDATED = '" + updatedDt + "'");

            query += string.Join(", ", updateFields);
            query += " WHERE SELLER_ID = " + r.SellerId;

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = query;
            genReq.isInsert = false;
            genReq.responseMessage = "Successfully updated";
            genReq.errorMessage = "Unable to update profile";

            GenericInsertUpdateResponse genResp = InsertUpdateData(genReq);

            resp.isSuccess = genResp.isSuccess;
            resp.Message = genResp.Message;
            resp.SellerId = r.SellerId;

            return resp;
        }

        // --------------------------------------------------------------------------------------------

        //Update Buyer Profile ------------------------------------------------------------------------

        public UpdateBuyerProfileResponse UpdateBuyerProfile(UpdateBuyerProfileRequest r)
        {
            UpdateBuyerProfileResponse resp = new UpdateBuyerProfileResponse();
            DateTime theDate = DateTime.Now;
            string updatedDt = theDate.ToString("yyyy-MM-dd H:mm:ss");

            string query = "UPDATE BUYERS SET ";
            List<string> updateFields = new List<string>();

            if (!string.IsNullOrEmpty(r.FirstName))
                updateFields.Add("BUYER_FIRST_NAME = '" + r.FirstName.Replace("'", "''") + "'");

            if (!string.IsNullOrEmpty(r.LastName))
                updateFields.Add("BUYER_LAST_NAME = '" + r.LastName.Replace("'", "''") + "'");

            if (!string.IsNullOrEmpty(r.UserName))
                updateFields.Add("BUYER_USERNAME = '" + r.UserName.Replace("'", "''") + "'");

            updateFields.Add("DATE_UPDATED = '" + updatedDt + "'");

            query += string.Join(", ", updateFields);
            query += " WHERE BUYER_ID = " + r.BuyerId;

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = query;
            genReq.isInsert = false;
            genReq.responseMessage = "Successfully updated";
            genReq.errorMessage = "Unable to update profile";

            GenericInsertUpdateResponse genResp = InsertUpdateData(genReq);

            resp.isSuccess = genResp.isSuccess;
            resp.Message = genResp.Message;
            resp.BuyerId = r.BuyerId;

            return resp;
        }

        // ----------------------------------------------------------------------------------------------













        //API LOGGING ---------------------------------------------------------------------------------
        public APILogResponse LogAPI(APILogRequest r)
        {
            APILogResponse resp = new APILogResponse();

            string safeApiName = (r.ApiName ?? "").Replace("'", "''");
            string safeRequest = (r.RequestData ?? "").Replace("'", "''");
            string safeResponse = (r.ResponseData ?? "").Replace("'", "''");

            string query = "INSERT INTO API_LOGS (API_NAME, API_LOG_TIME, REQUEST_DATA, RESPONSE_DATA) ";
            query += "VALUES ('" + safeApiName + "','" + r.LogTime + "','" +
                     safeRequest + "','" + safeResponse + "')";

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = query;
            genReq.isInsert = true;
            genReq.responseMessage = "Successfully logged API call";
            genReq.errorMessage = "Unable to log API call";

            GenericInsertUpdateResponse genResp = InsertUpdateData(genReq);

            resp.Message = genResp.Message;
            resp.isSuccess = genResp.isSuccess;
            resp.LogId = genResp.Id;

            return resp;
        }
        //----------------------------------------------------------------------------------------------------------












        // Products -------------------------------------------

        // Add Product --------------------------------------------------------------------------------------------
        public AddProductResponse AddProduct(AddProductRequest r)
        {
            AddProductResponse resp = new AddProductResponse();

            string activeStatus = GetSettingsValue("ACTIVE");
            string soldOutStatus = GetSettingsValue("SOLD_OUT");
            DateTime currentDate = DateTime.Now;
            string dateAdded = currentDate.ToString("yyyy-MM-dd HH:mm:ss");

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                int categoryId = GetOrCreateCategory(r.Category);

                if (categoryId <= 0)
                {
                    resp.isSuccess = false;
                    resp.Message = "Invalid category";
                    return resp;
                }

                string initialStatus = r.Quantity > 0 ? activeStatus : soldOutStatus;

                string productQuery = "INSERT INTO PRODUCTS (SELLER_ID, PRODUCT_NAME, PRODUCT_DESCRIPTION, PRODUCT_PRICE, " +
                                     "PRODUCT_QUANTITY, CATEGORY_ID, PRODUCT_BRAND, PRODUCT_STATUS, " +
                                     "DATE_ADDED) VALUES (" +
                                     r.SellerId + ", '" +
                                     r.ProductName.Replace("'", "''") + "', '" +
                                     r.ProductDescription.Replace("'", "''") + "', " +
                                     r.Price + ", " +
                                     r.Quantity + ", " +
                                     categoryId + ", '" +
                                     (string.IsNullOrEmpty(r.Brand) ? "" : r.Brand.Replace("'", "''")) + "', '" +
                                     initialStatus + "', '" +  // Use dynamic status instead of always ACTIVE
                                     dateAdded + "')";

                MySqlCommand cmd = new MySqlCommand(productQuery, conn);
                cmd.ExecuteNonQuery();

                int productId = int.Parse(cmd.LastInsertedId.ToString());
                resp.ProductId = productId;
                resp.isSuccess = true;
                resp.Message = "Product added successfully";

                conn.Close();
            }

            return resp;
        }


        private int GetOrCreateCategory(string categoryName)
        {
            string activeStatus = GetSettingsValue("ACTIVE");
            DateTime currentDate = DateTime.Now;
            string dateAdded = currentDate.ToString("yyyy-MM-dd HH:mm:ss");

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();

                string checkQuery = "SELECT CATEGORY_ID FROM PRODUCT_CATEGORY WHERE CATEGORY_NAME = '" +
                                   categoryName.Replace("'", "''") + "'";

                MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                var result = checkCmd.ExecuteScalar();

                if (result != null)
                {
                    int categoryId = Convert.ToInt32(result);
                    conn.Close();
                    return categoryId;
                }
                else
                {
                    string insertQuery = "INSERT INTO PRODUCT_CATEGORY (CATEGORY_NAME, DATE_ADDED) " +
                                        "VALUES ('" + categoryName.Replace("'", "''") + "', '" + dateAdded + "')";

                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn);
                    insertCmd.ExecuteNonQuery();

                    int newCategoryId = int.Parse(insertCmd.LastInsertedId.ToString());
                    conn.Close();
                    return newCategoryId;
                }
            }
        }
        // ---------------------------------------------------------------------------------------------------------

        // Update Product ------------------------------------------------------------------------------------------
        public UpdateProductResponse UpdateProduct(UpdateProductRequest r)
        {
            UpdateProductResponse resp = new UpdateProductResponse();
            resp.ProductId = r.ProductId;

            DateTime currentDate = DateTime.Now;
            string dateUpdated = currentDate.ToString("yyyy-MM-dd HH:mm:ss");

            int currentQuantity = 0;
            if (!r.Quantity.HasValue)
            {
                string getQtyQuery = "SELECT PRODUCT_QUANTITY FROM PRODUCTS WHERE PRODUCT_ID = " + r.ProductId;
                GenericGetDataResponse qtyData = GetData(getQtyQuery);
                if (qtyData.Data.Rows.Count > 0)
                {
                    currentQuantity = Convert.ToInt32(qtyData.Data.Rows[0]["PRODUCT_QUANTITY"]);
                }
            }

            string query = "UPDATE PRODUCTS SET ";
            List<string> updateFields = new List<string>();

            if (!string.IsNullOrEmpty(r.ProductName))
                updateFields.Add("PRODUCT_NAME = '" + r.ProductName.Replace("'", "''") + "'");

            if (!string.IsNullOrEmpty(r.ProductDescription))
                updateFields.Add("PRODUCT_DESCRIPTION = '" + r.ProductDescription.Replace("'", "''") + "'");

            if (r.Price.HasValue)
                updateFields.Add("PRODUCT_PRICE = " + r.Price.Value);

            if (r.Quantity.HasValue)
            {
                updateFields.Add("PRODUCT_QUANTITY = " + r.Quantity.Value);
                currentQuantity = r.Quantity.Value;
            }

            if (!string.IsNullOrEmpty(r.Category))
            {
                int categoryId = GetOrCreateCategory(r.Category);
                updateFields.Add("CATEGORY_ID = " + categoryId);
            }

            if (!string.IsNullOrEmpty(r.Brand))
                updateFields.Add("PRODUCT_BRAND = '" + r.Brand.Replace("'", "''") + "'");

            updateFields.Add("DATE_UPDATED = '" + dateUpdated + "'");

            query += string.Join(", ", updateFields);
            query += " WHERE PRODUCT_ID = " + r.ProductId + " AND SELLER_ID = " + r.SellerId;

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = query;
            genReq.isInsert = false;
            genReq.responseMessage = "Product updated successfully";
            genReq.errorMessage = "Unable to update product";

            GenericInsertUpdateResponse genResp = InsertUpdateData(genReq);

            if (genResp.isSuccess && r.Quantity.HasValue)
            {
                UpdateProductStatusBasedOnQuantity(r.ProductId);
            }

            resp.Message = genResp.Message;
            resp.isSuccess = genResp.isSuccess;

            return resp;
        }

        // ---------------------------------------------------------------------------------------------------------

        // Products Listing (Seller) -------------------------------------------------------------------------------
        public GetProductsResponse GetProducts(GetProductsRequest r)
        {
            GetProductsResponse resp = new GetProductsResponse();
            resp.ProductsList = new List<Product>();

            try
            {
                if (r.CategoryId.HasValue && r.CategoryId.Value > 0)
                {
                    string categoryQuery = "SELECT CATEGORY_NAME FROM PRODUCT_CATEGORY WHERE CATEGORY_ID = " + r.CategoryId.Value;
                    GenericGetDataResponse categoryData = GetData(categoryQuery);
                    if (categoryData.Data.Rows.Count > 0)
                    {
                        resp.CategoryName = categoryData.Data.Rows[0]["CATEGORY_NAME"].ToString();
                        resp.CategoryId = r.CategoryId.Value;
                    }
                }

                GenericGetDataResponse getData = SearchProductsQuery(
                    r.Keyword ?? "",
                    r.Status,
                    r.SellerId,
                    r.CategoryId
                );

                resp.ProductsList = MapDataTableToProductList(getData.Data);

                if (r.Page.HasValue && r.PageSize.HasValue && r.Page.Value > 0 && r.PageSize.Value > 0)
                {
                    resp.TotalCount = resp.ProductsList.Count;
                    resp.CurrentPage = r.Page.Value;
                    resp.PageSize = r.PageSize.Value;
                    resp.TotalPages = (int)Math.Ceiling((double)resp.TotalCount / r.PageSize.Value);

                    resp.ProductsList = resp.ProductsList
                        .Skip((r.Page.Value - 1) * r.PageSize.Value)
                        .Take(r.PageSize.Value)
                        .ToList();
                }
                else
                {
                    resp.TotalCount = resp.ProductsList.Count;
                }

                if (resp.ProductsList.Count > 0)
                {
                    resp.isSuccess = true;
                    resp.Message = "Products retrieved successfully";
                }
                else
                {
                    resp.isSuccess = true;
                    resp.Message = string.IsNullOrEmpty(r.Keyword)
                        ? "No products found"
                        : "No products found matching your search";
                }
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "Error retrieving products: " + ex.Message;
                resp.TotalCount = 0;
            }

            return resp;
        }

        // ---------------------------------------------------------------------------------------------------------

        // Get Products By Category --------------------------------------------------------------------------------
        public GetProductByCategoryResponse GetProductsByCategory(GetProductByCategoryRequest r)
        {
            GetProductByCategoryResponse resp = new GetProductByCategoryResponse();
            string categoryQuery = "SELECT * FROM PRODUCT_CATEGORY WHERE CATEGORY_ID = " + r.CategoryId;
            GenericGetDataResponse categoryData = GetData(categoryQuery);

            if (categoryData.Data.Rows.Count > 0)
            {
                DataRow dr = categoryData.Data.Rows[0];
                resp.CategoryId = int.Parse(dr["CATEGORY_ID"].ToString());
                resp.CategoryName = dr["CATEGORY_NAME"].ToString();
            }
            GenericGetDataResponse getData = SearchProductsQuery("", "ACTIVE", null, r.CategoryId);
            resp.ProductsList = MapDataTableToProductList(getData.Data);

            if (resp.ProductsList.Count > 0)
            {
                resp.isSuccess = true;
                resp.Message = "Products retrieved successfully";
                resp.TotalCount = resp.ProductsList.Count;
            }
            else
            {
                resp.isSuccess = true;
                resp.Message = "No products found in this category";
                resp.TotalCount = 0;
            }

            return resp;
        }

        // ---------------------------------------------------------------------------------------------------------

        // Delete Product ------------------------------------------------------------------------------------------
        public DeleteProductResponse DeleteProduct(DeleteProductRequest r)
        {
            DeleteProductResponse resp = new DeleteProductResponse();
            string inactiveStatus = GetSettingsValue("INACTIVE"); 
            DateTime deleteTime = DateTime.Now;
            string deleteDate = deleteTime.ToString("yyyy-MM-dd HH:mm:ss");

            int productSellerId = GetProductSellerId(r.ProductId);
            if (productSellerId != r.SellerId)
            {
                resp.isSuccess = false;
                resp.Message = "Product not found or does not belong to this seller";
                return resp;
            }

            string updateQuery = "UPDATE PRODUCTS SET PRODUCT_STATUS = '" + inactiveStatus +
                                "', DATE_UPDATED = '" + deleteDate +
                                "' WHERE PRODUCT_ID = " + r.ProductId +
                                " AND SELLER_ID = " + r.SellerId;

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(updateQuery, conn);
                int rowsUpdated = cmd.ExecuteNonQuery();

                if (rowsUpdated > 0)
                {
                    resp.isSuccess = true;
                    resp.Message = "Product successfully deleted at " + deleteDate;
                    resp.ProductId = r.ProductId;
                    resp.DeactivationDate = deleteDate;
                }
                else
                {
                    resp.isSuccess = false;
                    resp.Message = "Failed to delete product";
                }
                conn.Close();
            }

            return resp;
        }
        // ---------------------------------------------------------------------------------------------------------

        // Reviews and Comments -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        // Add Review ------------------------------------------------------------------------------------------------------
        public AddReviewResponse AddReview(AddReviewRequest r)
        {
            AddReviewResponse resp = new AddReviewResponse();
            string dateAdded = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string query = "INSERT INTO PRODUCT_REVIEWS (PRODUCT_ID, BUYER_ID, RATING, REVIEW_TEXT, DATE_ADDED) " +
                          "VALUES (" + r.ProductId + ", " + r.BuyerId + ", " + r.Rating + ", '" +
                          r.ReviewText.Replace("'", "''") + "', '" + dateAdded + "')";

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = query;
            genReq.isInsert = true;
            genReq.responseMessage = "Review added successfully";
            genReq.errorMessage = "Unable to add review";

            GenericInsertUpdateResponse genResp = InsertUpdateData(genReq);

            resp.isSuccess = genResp.isSuccess;
            resp.Message = genResp.Message;
            resp.ReviewId = genResp.Id;

            return resp;
        }

        // Get Product Reviews-----------------------------------------------------------------------------------------------
        public GetProductReviewsResponse GetProductReviews(GetProductReviewsRequest r)
        {
            GetProductReviewsResponse resp = new GetProductReviewsResponse();
            resp.Reviews = new List<ReviewItem>();

            string query = "SELECT r.*, CONCAT(b.BUYER_FIRST_NAME, ' ', b.BUYER_LAST_NAME) as BUYER_NAME " +
                          "FROM PRODUCT_REVIEWS r " +
                          "JOIN BUYERS b ON r.BUYER_ID = b.BUYER_ID " +
                          "WHERE r.PRODUCT_ID = " + r.ProductId + " " +
                          "ORDER BY r.DATE_ADDED DESC";

            GenericGetDataResponse getData = GetData(query);

            if (getData.isSuccess && getData.Data.Rows.Count > 0)
            {
                double totalRating = 0;

                foreach (DataRow dr in getData.Data.Rows)
                {
                    ReviewItem item = new ReviewItem();
                    item.ReviewId = Convert.ToInt32(dr["REVIEW_ID"]);
                    item.BuyerName = dr["BUYER_NAME"].ToString();
                    item.Rating = Convert.ToInt32(dr["RATING"]);
                    item.ReviewText = dr["REVIEW_TEXT"].ToString();
                    item.DateAdded = Convert.ToDateTime(dr["DATE_ADDED"]);

                    totalRating += item.Rating;
                    resp.Reviews.Add(item);
                }

                resp.AverageRating = totalRating / resp.Reviews.Count;
            }

            resp.isSuccess = true;
            resp.Message = "Reviews retrieved successfully";

            return resp;
        }

        // Add Comment ----------------------------------------------------------------------------------------------------------------
        public AddCommentResponse AddComment(AddCommentRequest r)
        {
            AddCommentResponse resp = new AddCommentResponse();
            string dateAdded = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string query = "INSERT INTO PRODUCT_COMMENTS (PRODUCT_ID, USER_ID, USER_TYPE, COMMENT_TEXT, DATE_ADDED) " +
                          "VALUES (" + r.ProductId + ", " + r.UserId + ", '" + r.UserType + "', '" +
                          r.CommentText.Replace("'", "''") + "', '" + dateAdded + "')";

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = query;
            genReq.isInsert = true;
            genReq.responseMessage = "Comment added successfully";
            genReq.errorMessage = "Unable to add comment";

            GenericInsertUpdateResponse genResp = InsertUpdateData(genReq);

            resp.isSuccess = genResp.isSuccess;
            resp.Message = genResp.Message;
            resp.CommentId = genResp.Id;

            return resp;
        }

        // Get Product Comments ---------------------------------------------------------------------------------------------------------------------------
        public GetProductCommentsResponse GetProductComments(GetProductCommentsRequest r)
        {
            GetProductCommentsResponse resp = new GetProductCommentsResponse();
            resp.Comments = new List<CommentItem>();

            string query = "SELECT c.*, " +
                          "CASE " +
                          "  WHEN c.USER_TYPE = 'BUYER' THEN (SELECT CONCAT(BUYER_FIRST_NAME, ' ', BUYER_LAST_NAME) FROM BUYERS WHERE BUYER_ID = c.USER_ID) " +
                          "  WHEN c.USER_TYPE = 'SELLER' THEN (SELECT CONCAT(SELLER_FIRST_NAME, ' ', SELLER_LAST_NAME) FROM SELLERS WHERE SELLER_ID = c.USER_ID) " +
                          "END as USER_NAME " +
                          "FROM PRODUCT_COMMENTS c " +
                          "WHERE c.PRODUCT_ID = " + r.ProductId + " " +
                          "ORDER BY c.DATE_ADDED DESC";

            GenericGetDataResponse getData = GetData(query);

            if (getData.isSuccess && getData.Data.Rows.Count > 0)
            {
                foreach (DataRow dr in getData.Data.Rows)
                {
                    CommentItem item = new CommentItem();
                    item.CommentId = Convert.ToInt32(dr["COMMENT_ID"]);
                    item.UserName = dr["USER_NAME"].ToString();
                    item.UserType = dr["USER_TYPE"].ToString();
                    item.CommentText = dr["COMMENT_TEXT"].ToString();
                    item.DateAdded = Convert.ToDateTime(dr["DATE_ADDED"]);

                    resp.Comments.Add(item);
                }
            }

            resp.isSuccess = true;
            resp.Message = "Comments retrieved successfully";

            return resp;
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


        // CART ----------------------------------------------------------------------------------------------------------------------------------------------------------

        // FUNCTIONS -----------------------------------------------------

        private (bool exists, int quantity, decimal price, string name, string status) CheckProductAvailability(int productId, string status)
        {
            string query = "SELECT PRODUCT_QUANTITY, PRODUCT_PRICE, PRODUCT_NAME, PRODUCT_STATUS FROM PRODUCTS WHERE PRODUCT_ID = " + productId;
            GenericGetDataResponse data = GetData(query);

            if (data.Data.Rows.Count > 0)
            {
                int qty = Convert.ToInt32(data.Data.Rows[0]["PRODUCT_QUANTITY"]);
                decimal price = Convert.ToDecimal(data.Data.Rows[0]["PRODUCT_PRICE"]);
                string name = data.Data.Rows[0]["PRODUCT_NAME"].ToString();
                string productStatus = data.Data.Rows[0]["PRODUCT_STATUS"].ToString();
                return (true, qty, price, name, productStatus);
            }
            return (false, 0, 0, "", "");
        }

        private (bool exists, int cartId, int quantity) CheckCartItem(int buyerId, int productId, string status)
        {
            string query = "SELECT CART_ID, QUANTITY FROM CART WHERE BUYER_ID = " + buyerId + " AND PRODUCT_ID = " + productId + " AND CART_STATUS = '" + status + "'";
            GenericGetDataResponse data = GetData(query);

            if (data.Data.Rows.Count > 0)
            {
                int cartId = Convert.ToInt32(data.Data.Rows[0]["CART_ID"]);
                int qty = Convert.ToInt32(data.Data.Rows[0]["QUANTITY"]);
                return (true, cartId, qty);
            }
            return (false, 0, 0);
        }

        private (int totalItems, decimal cartTotal) GetCartTotals(int buyerId, string status)
        {
            string query = "SELECT " +
                           "SUM(c.QUANTITY) as TotalItems, " +
                           "SUM(c.QUANTITY * p.PRODUCT_PRICE) as CartTotal " +
                           "FROM CART c " +
                           "JOIN PRODUCTS p ON c.PRODUCT_ID = p.PRODUCT_ID " +
                           "WHERE c.BUYER_ID = " + buyerId + " AND c.CART_STATUS = '" + status + "'";

            GenericGetDataResponse data = GetData(query);

            if (data.Data.Rows.Count > 0)
            {
                int items = data.Data.Rows[0]["TotalItems"] != DBNull.Value ? Convert.ToInt32(data.Data.Rows[0]["TotalItems"]) : 0;
                decimal total = data.Data.Rows[0]["CartTotal"] != DBNull.Value ? Convert.ToDecimal(data.Data.Rows[0]["CartTotal"]) : 0;
                return (items, total);
            }
            return (0, 0);
        }

        private bool VerifyCartOwnership(int cartId, int buyerId, string status)
        {
            string query = "SELECT COUNT(*) FROM CART WHERE CART_ID = " + cartId + " AND BUYER_ID = " + buyerId + " AND CART_STATUS = '" + status + "'";
            GenericGetDataResponse data = GetData(query);

            if (data.Data.Rows.Count > 0)
            {
                int count = Convert.ToInt32(data.Data.Rows[0][0]);
                return count > 0;
            }
            return false;
        }

        private GenericInsertUpdateResponse UpdateCartQuantity(int cartId, int newQuantity, string dateUpdated)
        {
            string query = "UPDATE CART SET QUANTITY = " + newQuantity + ", DATE_UPDATED = '" + dateUpdated + "' WHERE CART_ID = " + cartId;

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = query;
            genReq.isInsert = false;
            genReq.responseMessage = "Cart updated successfully";
            genReq.errorMessage = "Failed to update cart";

            return InsertUpdateData(genReq);
        }

        private GenericInsertUpdateResponse InsertCartItem(int buyerId, int productId, int quantity, string dateAdded, string status)
        {
            string query = "INSERT INTO CART (BUYER_ID, PRODUCT_ID, QUANTITY, DATE_ADDED, CART_STATUS) " +
                           "VALUES (" + buyerId + ", " + productId + ", " + quantity + ", '" + dateAdded + "', '" + status + "')";

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = query;
            genReq.isInsert = true;
            genReq.responseMessage = "Item added to cart";
            genReq.errorMessage = "Failed to add to cart";

            return InsertUpdateData(genReq);
        }

        private GenericInsertUpdateResponse DeleteCartItem(int cartId)
        {
            string query = "DELETE FROM CART WHERE CART_ID = " + cartId;

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = query;
            genReq.isInsert = false;
            genReq.responseMessage = "Item removed from cart";
            genReq.errorMessage = "Failed to remove from cart";

            return InsertUpdateData(genReq);
        }
        private GenericInsertUpdateResponse ClearBuyerCart(int buyerId)
        {
            string query = "DELETE FROM CART WHERE BUYER_ID = " + buyerId;

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = query;
            genReq.isInsert = false;
            genReq.responseMessage = "Cart cleared";
            genReq.errorMessage = "Failed to clear cart";

            return InsertUpdateData(genReq);
        }



        // ---------------------------------------------------------------

        // API -----------------------------------------------------------------------------------------------
        // Add to cart
        public AddToCartResponse AddToCart(AddToCartRequest r)
        {
            AddToCartResponse resp = new AddToCartResponse();
            string activeStatus = GetSettingsValue("ACTIVE");
            string soldOutStatus = GetSettingsValue("SOLD_OUT");
            DateTime currentDate = DateTime.Now;
            string dateAdded = currentDate.ToString("yyyy-MM-dd HH:mm:ss");

            var (productExists, availableStock, productPrice, productName, productStatus) = CheckProductAvailability(r.ProductId, activeStatus);

            if (!productExists)
            {
                resp.isSuccess = false;
                resp.Message = "Product not found";
                return resp;
            }
            if (productStatus == soldOutStatus || availableStock <= 0)
            {
                resp.isSuccess = false;
                resp.Message = "Product is sold out";
                return resp;
            }

            if (availableStock < r.Quantity)
            {
                resp.isSuccess = false;
                resp.Message = "Only " + availableStock + " items available in stock";
                return resp;
            }

            var (inCart, cartId, currentQty) = CheckCartItem(r.BuyerId, r.ProductId, activeStatus);

            if (inCart)
            {
                int newQty = currentQty + r.Quantity;

                if (availableStock < newQty)
                {
                    resp.isSuccess = false;
                    resp.Message = "Cannot add " + r.Quantity + " more. Only " + (availableStock - currentQty) + " additional items available";
                    return resp;
                }

                GenericInsertUpdateResponse updateResp = UpdateCartQuantity(cartId, newQty, dateAdded);
                resp.CartId = cartId;
            }
            else
            {
                GenericInsertUpdateResponse insertResp = InsertCartItem(r.BuyerId, r.ProductId, r.Quantity, dateAdded, activeStatus);
                resp.CartId = insertResp.Id;
            }

            var (totalItems, cartTotal) = GetCartTotals(r.BuyerId, activeStatus);
            resp.TotalItems = totalItems;
            resp.CartTotal = cartTotal;
            resp.isSuccess = true;
            resp.Message = "Item added to cart successfully";

            return resp;
        }

        // Get Cart
        public GetCartResponse GetCart(GetCartRequest r)
        {
            GetCartResponse resp = new GetCartResponse();
            resp.Items = new List<CartItem>();
            string activeStatus = GetSettingsValue("ACTIVE");

            string query = "SELECT c.CART_ID, c.PRODUCT_ID, c.QUANTITY, " +
                          "p.PRODUCT_NAME, p.PRODUCT_DESCRIPTION, p.PRODUCT_PRICE, " +
                          "p.PRODUCT_QUANTITY as AVAILABLE_STOCK, p.PRODUCT_BRAND, " +
                          "s.SELLER_STORE_NAME, s.SELLER_ID " +
                          "FROM CART c " +
                          "JOIN PRODUCTS p ON c.PRODUCT_ID = p.PRODUCT_ID " +
                          "LEFT JOIN SELLERS s ON p.SELLER_ID = s.SELLER_ID " +
                          "WHERE c.BUYER_ID = " + r.BuyerId + " AND c.CART_STATUS = '" + activeStatus + "' " +
                          "ORDER BY c.DATE_ADDED DESC";

            GenericGetDataResponse getData = GetData(query);

            if (getData.isSuccess && getData.Data.Rows.Count > 0)
            {
                decimal subtotal = 0;
                foreach (DataRow dr in getData.Data.Rows)
                {
                    CartItem item = new CartItem();
                    item.CartId = Convert.ToInt32(dr["CART_ID"]);
                    item.ProductId = Convert.ToInt32(dr["PRODUCT_ID"]);
                    item.ProductName = dr["PRODUCT_NAME"].ToString();
                    item.ProductDescription = dr["PRODUCT_DESCRIPTION"].ToString();
                    item.Price = Convert.ToDecimal(dr["PRODUCT_PRICE"]);
                    item.Quantity = Convert.ToInt32(dr["QUANTITY"]);
                    item.Subtotal = item.Price * item.Quantity;
                    item.AvailableStock = Convert.ToInt32(dr["AVAILABLE_STOCK"]);
                    item.Brand = dr["PRODUCT_BRAND"]?.ToString() ?? "";
                    item.StoreName = dr["SELLER_STORE_NAME"]?.ToString() ?? "";
                    item.SellerId = dr["SELLER_ID"] != DBNull.Value ? Convert.ToInt32(dr["SELLER_ID"]) : 0;

                    subtotal += item.Subtotal;
                    resp.Items.Add(item);
                }

                resp.BuyerId = r.BuyerId;
                resp.TotalItems = resp.Items.Sum(i => i.Quantity); // Already calculating TotalItems
                resp.Subtotal = subtotal;
                resp.isSuccess = true;
                resp.Message = "Cart retrieved successfully";
            }
            else
            {
                resp.isSuccess = true;
                resp.Message = "Cart is empty";
                resp.TotalItems = 0; // Returns 0 when empty
                resp.Subtotal = 0;
            }

            return resp;
        }

        // Update Cart Item
        public UpdateCartItemResponse UpdateCartItem(UpdateCartItemRequest r)
        {
            UpdateCartItemResponse resp = new UpdateCartItemResponse();
            string activeStatus = GetSettingsValue("ACTIVE");
            DateTime currentDate = DateTime.Now;
            string dateUpdated = currentDate.ToString("yyyy-MM-dd HH:mm:ss");

            if (!VerifyCartOwnership(r.CartId, r.BuyerId, activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Cart item not found";
                return resp;
            }

            string getCartQuery = "SELECT c.PRODUCT_ID, p.PRODUCT_QUANTITY as AVAILABLE_STOCK, p.PRODUCT_PRICE " +
                                 "FROM CART c " +
                                 "JOIN PRODUCTS p ON c.PRODUCT_ID = p.PRODUCT_ID " +
                                 "WHERE c.CART_ID = " + r.CartId;

            GenericGetDataResponse cartData = GetData(getCartQuery);

            int productId = Convert.ToInt32(cartData.Data.Rows[0]["PRODUCT_ID"]);
            int availableStock = Convert.ToInt32(cartData.Data.Rows[0]["AVAILABLE_STOCK"]);
            decimal productPrice = Convert.ToDecimal(cartData.Data.Rows[0]["PRODUCT_PRICE"]);

            if (r.NewQuantity <= 0)
            {
                GenericInsertUpdateResponse deleteResp = DeleteCartItem(r.CartId);
                resp.Message = "Item removed from cart";
            }
            else
            {
                if (availableStock < r.NewQuantity)
                {
                    resp.isSuccess = false;
                    resp.Message = "Only " + availableStock + " items available in stock";
                    return resp;
                }

                GenericInsertUpdateResponse updateResp = UpdateCartQuantity(r.CartId, r.NewQuantity, dateUpdated);
                resp.ItemSubtotal = productPrice * r.NewQuantity;
                resp.NewQuantity = r.NewQuantity;
                resp.Message = "Cart updated successfully";
            }

            var (totalItems, cartTotal) = GetCartTotals(r.BuyerId, activeStatus);
            resp.TotalItems = totalItems;
            resp.CartTotal = cartTotal;
            resp.isSuccess = true;
            resp.CartId = r.CartId;

            return resp;
        }

        // Remove from Cart
        public RemoveFromCartResponse RemoveFromCart(RemoveFromCartRequest r)
        {
            RemoveFromCartResponse resp = new RemoveFromCartResponse();
            string activeStatus = GetSettingsValue("ACTIVE");

            if (!VerifyCartOwnership(r.CartId, r.BuyerId, activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Cart item not found";
                return resp;
            }

            GenericInsertUpdateResponse deleteResp = DeleteCartItem(r.CartId);

            var (totalItems, cartTotal) = GetCartTotals(r.BuyerId, activeStatus);
            resp.TotalItems = totalItems;
            resp.CartTotal = cartTotal;
            resp.isSuccess = true;
            resp.Message = "Item removed from cart successfully";
            resp.CartId = r.CartId;

            return resp;
        }

        // Clear Cart
        public ClearCartResponse ClearCart(ClearCartRequest r)
        {
            ClearCartResponse resp = new ClearCartResponse();
            DateTime currentDate = DateTime.Now;

            GenericInsertUpdateResponse clearResp = ClearBuyerCart(r.BuyerId);

            resp.isSuccess = true;
            resp.Message = "Cart cleared successfully";
            resp.BuyerId = r.BuyerId;
            resp.ClearedDate = currentDate;

            return resp;
        }

        // ---------------------------------------------------------------------------------------------------

        // Voucher ------------------------------------------------------------

        //Create Voucher
        public CreateVoucherResponse CreateVoucher(CreateVoucherRequest r)
        {
            CreateVoucherResponse resp = new CreateVoucherResponse();

            // Check if voucher code already exists
            if (CheckVoucherCodeExists(r.VoucherCode))
            {
                resp.isSuccess = false;
                resp.Message = "Voucher code already exists";
                return resp;
            }

            string activeStatus = GetSettingsValue("ACTIVE");
            DateTime currentDate = DateTime.Now;
            string dateAdded = currentDate.ToString("yyyy-MM-dd HH:mm:ss");

            string query = "INSERT INTO VOUCHERS (SELLER_ID, VOUCHER_CODE, VOUCHER_NAME, VOUCHER_DESCRIPTION, " +
                           "DISCOUNT_AMOUNT, MINIMUM_SPEND, VALID_FROM, VALID_TO, MAX_USES, USED_COUNT, " +
                           "VOUCHER_STATUS, DATE_ADDED) VALUES (" +
                           r.SellerId + ", '" +
                           r.VoucherCode.Replace("'", "''") + "', '" +
                           r.VoucherName.Replace("'", "''") + "', '" +
                           (r.VoucherDescription ?? "").Replace("'", "''") + "', " +
                           r.DiscountAmount + ", " +
                           r.MinimumSpend + ", " +
                           "'" + r.ValidFrom.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                           "'" + r.ValidTo.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                           r.MaxUses + ", 0, '" +
                           activeStatus + "', '" +
                           dateAdded + "')";

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = query;
            genReq.isInsert = true;
            genReq.responseMessage = "Voucher created successfully";
            genReq.errorMessage = "Unable to create voucher";

            GenericInsertUpdateResponse genResp = InsertUpdateData(genReq);

            resp.isSuccess = genResp.isSuccess;
            resp.Message = genResp.Message;
            resp.VoucherId = genResp.Id;
            resp.VoucherCode = r.VoucherCode;

            return resp;
        }


        // Delete Voucher
        public DeleteVoucherResponse DeleteVoucher(DeleteVoucherRequest r)
        {
            DeleteVoucherResponse resp = new DeleteVoucherResponse();
            string inactiveStatus = GetSettingsValue("INACTIVE");
            DateTime deleteTime = DateTime.Now;
            string deleteDate = deleteTime.ToString("yyyy-MM-dd HH:mm:ss");

            // Check if voucher belongs to seller
            if (!CheckVoucherBelongsToSeller(r.VoucherId, r.SellerId))
            {
                resp.isSuccess = false;
                resp.Message = "Voucher not found or does not belong to this seller";
                return resp;
            }

            string updateQuery = "UPDATE VOUCHERS SET VOUCHER_STATUS = '" + inactiveStatus + "', " +
                                "DATE_UPDATED = '" + deleteDate + "' WHERE VOUCHER_ID = " + r.VoucherId;

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = updateQuery;
            genReq.isInsert = false;
            genReq.responseMessage = "Voucher successfully deleted at " + deleteDate;
            genReq.errorMessage = "Failed to delete voucher";

            GenericInsertUpdateResponse genResp = InsertUpdateData(genReq);

            resp.isSuccess = genResp.isSuccess;
            resp.Message = genResp.Message;
            resp.VoucherId = r.VoucherId;
            resp.DeletionDate = deleteDate;

            return resp;
        }

        public GetVouchersResponse GetVouchers(GetVouchersRequest r)
        {
            GetVouchersResponse resp = new GetVouchersResponse();
            resp.Vouchers = new List<VoucherItem>();

            try
            {
                string query = "SELECT VOUCHER_ID, SELLER_ID, VOUCHER_CODE, VOUCHER_NAME, VOUCHER_DESCRIPTION, " +
                               "DISCOUNT_AMOUNT, MINIMUM_SPEND, VALID_FROM, VALID_TO, MAX_USES, USED_COUNT, " +
                               "VOUCHER_STATUS, DATE_ADDED, DATE_UPDATED " +
                               "FROM VOUCHERS WHERE 1=1";

                if (r.SellerId.HasValue && r.SellerId.Value > 0)
                {
                    query += " AND SELLER_ID = " + r.SellerId.Value;
                }

                if (!string.IsNullOrEmpty(r.Status) && r.Status != "ALL")
                {
                    string statusValue = GetSettingsValue(r.Status);
                    query += " AND VOUCHER_STATUS = '" + statusValue + "'";
                }

                query += " ORDER BY DATE_ADDED DESC";

                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        VoucherItem v = new VoucherItem();
                        v.VoucherId = reader.GetInt32("VOUCHER_ID");
                        v.SellerId = reader.GetInt32("SELLER_ID");
                        v.VoucherCode = reader.GetString("VOUCHER_CODE");
                        v.VoucherName = reader.GetString("VOUCHER_NAME");
                        v.VoucherDescription = reader.IsDBNull(reader.GetOrdinal("VOUCHER_DESCRIPTION")) ? "" : reader.GetString("VOUCHER_DESCRIPTION");
                        v.DiscountAmount = reader.GetDecimal("DISCOUNT_AMOUNT");
                        v.MinimumSpend = reader.GetDecimal("MINIMUM_SPEND");
                        v.ValidFrom = reader.GetDateTime("VALID_FROM");
                        v.ValidTo = reader.GetDateTime("VALID_TO");
                        v.MaxUses = reader.GetInt32("MAX_USES");
                        v.UsedCount = reader.GetInt32("USED_COUNT");
                        v.VoucherStatus = reader.GetString("VOUCHER_STATUS");
                        v.DateAdded = reader.GetDateTime("DATE_ADDED");

                        if (!reader.IsDBNull(reader.GetOrdinal("DATE_UPDATED")))
                        {
                            v.DateUpdated = reader.GetDateTime("DATE_UPDATED");
                        }

                        resp.Vouchers.Add(v);
                    }
                    reader.Close();
                    conn.Close();
                }

                resp.isSuccess = true;
                resp.Message = "Vouchers retrieved successfully";
                resp.TotalCount = resp.Vouchers.Count;
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = ex.Message;
                resp.Vouchers = new List<VoucherItem>();
                resp.TotalCount = 0;
            }

            return resp;
        }

        // Get Active Vouchers
        public GetVouchersResponse GetActiveVouchers()
        {
            GetVouchersResponse resp = new GetVouchersResponse();
            resp.Vouchers = new List<VoucherItem>();

            try
            {
                string activeStatus = GetSettingsValue("ACTIVE");
                DateTime now = DateTime.Now;
                string currentDate = now.ToString("yyyy-MM-dd HH:mm:ss");

                string query = "SELECT VOUCHER_ID, SELLER_ID, VOUCHER_CODE, VOUCHER_NAME, VOUCHER_DESCRIPTION, " +
                               "DISCOUNT_AMOUNT, MINIMUM_SPEND, VALID_FROM, VALID_TO, MAX_USES, USED_COUNT, " +
                               "DATE_ADDED " +
                               "FROM VOUCHERS WHERE VOUCHER_STATUS = '" + activeStatus + "' " +
                               "AND VALID_FROM <= '" + currentDate + "' " +
                               "AND VALID_TO >= '" + currentDate + "' " +
                               "AND USED_COUNT < MAX_USES " +
                               "ORDER BY VALID_TO ASC";

                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        VoucherItem v = new VoucherItem();
                        v.VoucherId = reader.GetInt32("VOUCHER_ID");
                        v.SellerId = reader.GetInt32("SELLER_ID");
                        v.VoucherCode = reader.GetString("VOUCHER_CODE");
                        v.VoucherName = reader.GetString("VOUCHER_NAME");
                        v.VoucherDescription = reader.IsDBNull(reader.GetOrdinal("VOUCHER_DESCRIPTION")) ? "" : reader.GetString("VOUCHER_DESCRIPTION");
                        v.DiscountAmount = reader.GetDecimal("DISCOUNT_AMOUNT");
                        v.MinimumSpend = reader.GetDecimal("MINIMUM_SPEND");
                        v.ValidFrom = reader.GetDateTime("VALID_FROM");
                        v.ValidTo = reader.GetDateTime("VALID_TO");
                        v.MaxUses = reader.GetInt32("MAX_USES");
                        v.UsedCount = reader.GetInt32("USED_COUNT");
                        v.DateAdded = reader.GetDateTime("DATE_ADDED");
                        v.VoucherStatus = "A"; // Active
                        v.DateUpdated = null;

                        resp.Vouchers.Add(v);
                    }
                    reader.Close();
                    conn.Close();
                }

                resp.isSuccess = true;
                resp.Message = "Active vouchers retrieved successfully";
                resp.TotalCount = resp.Vouchers.Count;
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = ex.Message;
                resp.Vouchers = new List<VoucherItem>();
                resp.TotalCount = 0;
            }

            return resp;
        }

        // Validate Voucher
        public ValidateVoucherResponse ValidateVoucher(ValidateVoucherRequest r)
        {
            ValidateVoucherResponse resp = new ValidateVoucherResponse();
            if (!CheckVoucherActive(r.VoucherCode))
            {
                resp.isValid = false;
                resp.Message = "Invalid or expired voucher code";
                return resp;
            }

            string query = "SELECT VOUCHER_ID, VOUCHER_NAME, DISCOUNT_AMOUNT, MINIMUM_SPEND, MAX_USES, USED_COUNT " +
                           "FROM VOUCHERS WHERE VOUCHER_CODE = '" + r.VoucherCode.Replace("'", "''") + "'";

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    int voucherId = reader.GetInt32(0);
                    string voucherName = reader.GetString(1);
                    decimal discountAmount = reader.GetDecimal(2);
                    decimal minSpend = reader.GetDecimal(3);
                    int maxUses = reader.GetInt32(4);
                    int usedCount = reader.GetInt32(5);

                    reader.Close();

                    if (r.CartTotal < minSpend)
                    {
                        resp.isValid = false;
                        resp.Message = "Minimum spend of " + minSpend.ToString("C") + " required";
                        return resp;
                    }

                    if (usedCount >= maxUses)
                    {
                        resp.isValid = false;
                        resp.Message = "Voucher usage limit has been reached";
                        return resp;
                    }

                    resp.isValid = true;
                    resp.Message = "Voucher is valid";
                    resp.VoucherCode = r.VoucherCode;
                    resp.VoucherName = voucherName;
                    resp.DiscountAmount = discountAmount;
                    resp.MinimumSpend = minSpend;
                }
                else
                {
                    resp.isValid = false;
                    resp.Message = "Voucher not found";
                }

                conn.Close();
            }

            return resp;
        }

        // Apply Voucher
        public ApplyVoucherResponse ApplyVoucher(ApplyVoucherRequest r)
        {
            ApplyVoucherResponse resp = new ApplyVoucherResponse();
            ValidateVoucherRequest validateReq = new ValidateVoucherRequest
            {
                VoucherCode = r.VoucherCode,
                CartTotal = r.CartTotal
            };

            ValidateVoucherResponse validateResp = ValidateVoucher(validateReq);

            if (!validateResp.isValid)
            {
                resp.isSuccess = false;
                resp.Message = validateResp.Message;
                return resp;
            }

            int voucherId = GetVoucherIdFromCode(r.VoucherCode);

            string getDetailsQuery = "SELECT VOUCHER_NAME, DISCOUNT_AMOUNT FROM VOUCHERS WHERE VOUCHER_ID = " + voucherId;

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(getDetailsQuery, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string voucherName = reader.GetString(0);
                    decimal discountAmount = reader.GetDecimal(1);
                    reader.Close();

                    decimal finalTotal = r.CartTotal - discountAmount;
                    if (finalTotal < 0) finalTotal = 0;

                    UpdateVoucherUsedCount(voucherId);

                    resp.isSuccess = true;
                    resp.Message = "Voucher applied successfully";
                    resp.VoucherId = voucherId;
                    resp.VoucherCode = r.VoucherCode;
                    resp.VoucherName = voucherName;
                    resp.OriginalTotal = r.CartTotal;
                    resp.DiscountAmount = discountAmount;
                    resp.FinalTotal = finalTotal;
                    string getMinQuery = "SELECT MINIMUM_SPEND FROM VOUCHERS WHERE VOUCHER_ID = " + voucherId;
                    MySqlCommand minCmd = new MySqlCommand(getMinQuery, conn);
                    object minResult = minCmd.ExecuteScalar();
                    resp.MinimumSpend = minResult != null ? Convert.ToDecimal(minResult) : 0;
                }
                else
                {
                    resp.isSuccess = false;
                    resp.Message = "Voucher not found";
                }
                conn.Close();
            }

            return resp;
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------

        // E-WALLET FUNCTIONS ------------------------------------------------------------------------------------------------------------------------

        private void SendWalletVerificationEmail(string toEmail, string firstName, string verificationCode)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("neilsalcedo29@gmail.com", "E-Wallet Registration");
                    mail.To.Add(toEmail);
                    mail.Subject = "Wallet Verification Code";

                    mail.Body = $"Hello {firstName},<br><br>" +
                               $"You have requested to create an e-wallet for your account.<br><br>" +
                               $"<strong>Your verification code is: {verificationCode}</strong><br><br>" +
                               $"This code will expire in 10 minutes.<br>" +
                               $"Please enter this code to create your wallet.<br><br>" +
                               $"If you did not request this, please ignore this email.<br><br>" +
                               $"Thank you!";

                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("neilsalcedo29@gmail.com", "fnjf zcnd wnkg naua");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
        }

        // Get Wallet Balance
        private decimal GetWalletBalance(int userId, string userType)
        {
            string query = "SELECT BALANCE FROM WALLETS WHERE USER_ID = " + userId +
                           " AND USER_TYPE = '" + userType + "' AND STATUS = '" + GetSettingsValue("ACTIVE") + "'";

            GenericGetDataResponse getData = GetData(query);

            if (getData.isSuccess && getData.Data.Rows.Count > 0)
            {
                return Convert.ToDecimal(getData.Data.Rows[0]["BALANCE"]);
            }

            return 0;
        }

        // Update Wallet Balance
        private bool UpdateWalletBalance(int walletId, decimal newBalance)
        {
            DateTime currentDate = DateTime.Now;
            string dateUpdated = currentDate.ToString("yyyy-MM-dd HH:mm:ss");

            string query = "UPDATE WALLETS SET BALANCE = " + newBalance +
                           ", DATE_UPDATED = '" + dateUpdated +
                           "' WHERE WALLET_ID = " + walletId;

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = query;
            genReq.isInsert = false;
            genReq.responseMessage = "Balance updated";
            genReq.errorMessage = "Failed to update balance";

            GenericInsertUpdateResponse genResp = InsertUpdateData(genReq);

            return genResp.isSuccess;
        }

        // Add Transaction Record
        private bool AddTransaction(int userId, string userType, string transactionType,
            decimal amount, decimal balanceBefore, decimal balanceAfter, string remarks)
        {
            DateTime currentDate = DateTime.Now;
            string transactionDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss");

            string query = "INSERT INTO WALLET_TRANSACTIONS (USER_ID, USER_TYPE, TRANSACTION_TYPE, " +
                           "AMOUNT, BALANCE_BEFORE, BALANCE_AFTER, REMARKS, TRANSACTION_DATE) VALUES (" +
                           userId + ", '" + userType + "', '" + transactionType + "', " +
                           amount + ", " + balanceBefore + ", " + balanceAfter + ", '" +
                           (remarks ?? "").Replace("'", "''") + "', '" + transactionDate + "')";

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = query;
            genReq.isInsert = true;
            genReq.responseMessage = "Transaction recorded";
            genReq.errorMessage = "Failed to record transaction";

            GenericInsertUpdateResponse genResp = InsertUpdateData(genReq);

            return genResp.isSuccess;
        }


        //E-Wallet APIs ---------------------------------------------------------------------------------------------------------------------------------------

        // Request Wallet Code
        public RequestWalletCodeResponse RequestWalletCode(RequestWalletCodeRequest r)
        {
            RequestWalletCodeResponse resp = new RequestWalletCodeResponse();
            string activeStatus = GetSettingsValue("ACTIVE");
            string checkUserQuery = "";
            if (r.UserType.ToUpper() == "SELLER")
            {
                checkUserQuery = "SELECT SELLER_ID, SELLER_EMAIL, SELLER_FIRST_NAME FROM SELLERS WHERE SELLER_ID = " + r.UserId +
                                " AND SELLER_EMAIL = '" + r.Email.Replace("'", "''") + "' AND SELLER_STATUS = '" + activeStatus + "'";
            }
            else if (r.UserType.ToUpper() == "BUYER")
            {
                checkUserQuery = "SELECT BUYER_ID, BUYER_EMAIL, BUYER_FIRST_NAME FROM BUYERS WHERE BUYER_ID = " + r.UserId +
                                " AND BUYER_EMAIL = '" + r.Email.Replace("'", "''") + "' AND BUYER_STATUS = '" + activeStatus + "'";
            }
            else
            {
                resp.isSuccess = false;
                resp.Message = "Invalid user type";
                return resp;
            }

            GenericGetDataResponse userData = GetData(checkUserQuery);

            if (!userData.isSuccess || userData.Data.Rows.Count == 0)
            {
                resp.isSuccess = false;
                resp.Message = "User not found or email does not match";
                return resp;
            }
            string checkWalletQuery = "SELECT WALLET_ID FROM WALLETS WHERE USER_ID = " + r.UserId +
                                     " AND USER_TYPE = '" + r.UserType + "'";

            GenericGetDataResponse walletData = GetData(checkWalletQuery);

            if (walletData.isSuccess && walletData.Data.Rows.Count > 0)
            {
                resp.isSuccess = false;
                resp.Message = "Wallet already exists for this user";
                return resp;
            }

            string inactiveStatus = GetSettingsValue("INACTIVE");
            string deactivateQuery = "UPDATE WALLET_CODES SET STATUS = '" + inactiveStatus +
                                    "' WHERE USER_ID = " + r.UserId + " AND USER_TYPE = '" + r.UserType +
                                    "' AND STATUS = '" + activeStatus + "'";

            GenericInsertUpdateRequest deactivateReq = new GenericInsertUpdateRequest();
            deactivateReq.query = deactivateQuery;
            deactivateReq.isInsert = false;
            InsertUpdateData(deactivateReq);


            string verificationCode = new Random().Next(100000, 999999).ToString();
            DateTime currentDate = DateTime.Now;
            string dateCreated = currentDate.ToString("yyyy-MM-dd HH:mm:ss");
            string expiryDate = currentDate.AddMinutes(10).ToString("yyyy-MM-dd HH:mm:ss");

            string insertQuery = "INSERT INTO WALLET_CODES (USER_ID, USER_TYPE, EMAIL, VERIFICATION_CODE, " +
                                "EXPIRY_DATE, STATUS, DATE_CREATED) VALUES (" +
                                r.UserId + ", '" + r.UserType + "', '" + r.Email.Replace("'", "''") + "', '" +
                                verificationCode + "', '" + expiryDate + "', '" + activeStatus + "', '" + dateCreated + "')";

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = insertQuery;
            genReq.isInsert = true;
            genReq.responseMessage = "Verification code sent";
            genReq.errorMessage = "Failed to generate code";

            GenericInsertUpdateResponse genResp = InsertUpdateData(genReq);

            if (genResp.isSuccess)
            {
                string firstName = userData.Data.Rows[0][2].ToString();
                SendWalletVerificationEmail(r.Email, firstName, verificationCode);

                resp.isSuccess = true;
                resp.Message = "Verification code sent to your email";
                resp.VerificationCode = verificationCode;
            }
            else
            {
                resp.isSuccess = false;
                resp.Message = "Failed to generate verification code";
            }

            return resp;
        }

        // Create Wallet
        public CreateWalletResponse CreateWallet(CreateWalletRequest r)
        {
            CreateWalletResponse resp = new CreateWalletResponse();
            string activeStatus = GetSettingsValue("ACTIVE");
            DateTime currentDate = DateTime.Now;
            string dateCreated = currentDate.ToString("yyyy-MM-dd HH:mm:ss");

            string checkWalletQuery = "SELECT WALLET_ID FROM WALLETS WHERE USER_ID = " + r.UserId +
                                     " AND USER_TYPE = '" + r.UserType + "'";

            GenericGetDataResponse existingWallet = GetData(checkWalletQuery);

            if (existingWallet.isSuccess && existingWallet.Data.Rows.Count > 0)
            {
                resp.isSuccess = false;
                resp.Message = "Wallet already exists for this user";
                return resp;
            }

            string verifyQuery = "SELECT WALLET_CODE_ID FROM WALLET_CODES WHERE USER_ID = " + r.UserId +
                                " AND USER_TYPE = '" + r.UserType + "' AND EMAIL = '" + r.Email.Replace("'", "''") +
                                "' AND VERIFICATION_CODE = '" + r.VerificationCode + "' AND STATUS = '" + activeStatus +
                                "' AND EXPIRY_DATE > NOW()";

            GenericGetDataResponse verifyData = GetData(verifyQuery);

            if (!verifyData.isSuccess || verifyData.Data.Rows.Count == 0)
            {
                resp.isSuccess = false;
                resp.Message = "Invalid or expired verification code";
                return resp;
            }

            int walletCodeId = Convert.ToInt32(verifyData.Data.Rows[0]["WALLET_CODE_ID"]);
            string insertWalletQuery = "INSERT INTO WALLETS (USER_ID, USER_TYPE, BALANCE, STATUS, DATE_CREATED) " +
                                      "VALUES (" + r.UserId + ", '" + r.UserType + "', 0, '" + activeStatus + "', '" + dateCreated + "')";

            GenericInsertUpdateRequest genReq = new GenericInsertUpdateRequest();
            genReq.query = insertWalletQuery;
            genReq.isInsert = true;
            genReq.responseMessage = "Wallet created successfully";
            genReq.errorMessage = "Failed to create wallet";

            GenericInsertUpdateResponse genResp = InsertUpdateData(genReq);

            if (genResp.isSuccess)
            {
                string usedStatus = GetSettingsValue("USED");
                string updateCodeQuery = "UPDATE WALLET_CODES SET STATUS = '" + usedStatus +
                                        "', DATE_USED = '" + dateCreated + "' WHERE WALLET_CODE_ID = " + walletCodeId;

                GenericInsertUpdateRequest updateReq = new GenericInsertUpdateRequest();
                updateReq.query = updateCodeQuery;
                updateReq.isInsert = false;
                InsertUpdateData(updateReq);

                resp.isSuccess = true;
                resp.Message = "Wallet created successfully!";
                resp.WalletId = genResp.Id;
                resp.Balance = 0;
                resp.DateCreated = currentDate;
            }
            else
            {
                resp.isSuccess = false;
                resp.Message = genResp.Message;
            }

            return resp;
        }




        // Deposit to Wallet
        public WalletDepositResponse DepositToWallet(WalletDepositRequest r)
        {
            WalletDepositResponse resp = new WalletDepositResponse();

            if (r.Amount <= 0)
            {
                resp.isSuccess = false;
                resp.Message = "Deposit amount must be greater than 0";
                return resp;
            }

            // Check if wallet exists
            string checkWalletQuery = "SELECT WALLET_ID, BALANCE FROM WALLETS WHERE USER_ID = " + r.UserId +
                                     " AND USER_TYPE = '" + r.UserType + "' AND STATUS = '" + GetSettingsValue("ACTIVE") + "'";

            GenericGetDataResponse walletData = GetData(checkWalletQuery);

            if (!walletData.isSuccess || walletData.Data.Rows.Count == 0)
            {
                resp.isSuccess = false;
                resp.Message = "No wallet found. Please create a wallet first.";
                return resp;
            }

            int walletId = Convert.ToInt32(walletData.Data.Rows[0]["WALLET_ID"]);
            decimal balanceBefore = Convert.ToDecimal(walletData.Data.Rows[0]["BALANCE"]);
            decimal balanceAfter = balanceBefore + r.Amount;

            if (UpdateWalletBalance(walletId, balanceAfter))
            {
                if (AddTransaction(r.UserId, r.UserType, "DEPOSIT", r.Amount, balanceBefore, balanceAfter, r.ReferenceNumber))
                {
                    resp.isSuccess = true;
                    resp.Message = "Deposit successful!";
                    resp.NewBalance = balanceAfter;
                    resp.AmountDeposited = r.Amount;
                    resp.TransactionDate = DateTime.Now;
                }
                else
                {
                    UpdateWalletBalance(walletId, balanceBefore);
                    resp.isSuccess = false;
                    resp.Message = "Failed to record transaction";
                }
            }
            else
            {
                resp.isSuccess = false;
                resp.Message = "Failed to update balance";
            }

            return resp;
        }


        // Withdraw from Wallet
        public WalletWithdrawResponse WithdrawFromWallet(WalletWithdrawRequest r)
        {
            WalletWithdrawResponse resp = new WalletWithdrawResponse();

            if (r.Amount <= 0)
            {
                resp.isSuccess = false;
                resp.Message = "Withdrawal amount must be greater than 0";
                return resp;
            }

            // Check if wallet exists and get balance
            string checkWalletQuery = "SELECT WALLET_ID, BALANCE FROM WALLETS WHERE USER_ID = " + r.UserId +
                                     " AND USER_TYPE = '" + r.UserType + "' AND STATUS = '" + GetSettingsValue("ACTIVE") + "'";

            GenericGetDataResponse walletData = GetData(checkWalletQuery);

            if (!walletData.isSuccess || walletData.Data.Rows.Count == 0)
            {
                resp.isSuccess = false;
                resp.Message = "No wallet found. Please create a wallet first.";
                return resp;
            }

            int walletId = Convert.ToInt32(walletData.Data.Rows[0]["WALLET_ID"]);
            decimal balanceBefore = Convert.ToDecimal(walletData.Data.Rows[0]["BALANCE"]);

            if (balanceBefore < r.Amount)
            {
                resp.isSuccess = false;
                resp.Message = "Insufficient balance";
                return resp;
            }

            decimal balanceAfter = balanceBefore - r.Amount;

            if (UpdateWalletBalance(walletId, balanceAfter))
            {
                string remarks = "Withdrawal request";

                if (AddTransaction(r.UserId, r.UserType, "WITHDRAWAL", r.Amount, balanceBefore, balanceAfter, remarks))
                {
                    resp.isSuccess = true;
                    resp.Message = "Withdrawal successful!";
                    resp.NewBalance = balanceAfter;
                    resp.AmountWithdrawn = r.Amount;
                    resp.TransactionDate = DateTime.Now;
                }
                else
                {
                    UpdateWalletBalance(walletId, balanceBefore);
                    resp.isSuccess = false;
                    resp.Message = "Failed to record transaction";
                }
            }
            else
            {
                resp.isSuccess = false;
                resp.Message = "Failed to update balance";
            }

            return resp;
        }

        // Get Wallet Balance
        public GetWalletBalanceResponse GetWalletBalance(GetWalletBalanceRequest r)
        {
            GetWalletBalanceResponse resp = new GetWalletBalanceResponse();

            string checkWalletQuery = "SELECT BALANCE FROM WALLETS WHERE USER_ID = " + r.UserId +
                                     " AND USER_TYPE = '" + r.UserType + "' AND STATUS = '" + GetSettingsValue("ACTIVE") + "'";

            GenericGetDataResponse walletData = GetData(checkWalletQuery);

            decimal balance = 0;
            bool walletExists = walletData.isSuccess && walletData.Data.Rows.Count > 0;

            if (walletExists)
            {
                balance = Convert.ToDecimal(walletData.Data.Rows[0]["BALANCE"]);
            }

            string userName = "";
            if (r.UserType.ToUpper() == "SELLER")
            {
                string nameQuery = "SELECT SELLER_USERNAME FROM SELLERS WHERE SELLER_ID = " + r.UserId;
                GenericGetDataResponse nameData = GetData(nameQuery);
                if (nameData.isSuccess && nameData.Data.Rows.Count > 0)
                    userName = nameData.Data.Rows[0][0].ToString();
            }
            else if (r.UserType.ToUpper() == "BUYER")
            {
                string nameQuery = "SELECT BUYER_USERNAME FROM BUYERS WHERE BUYER_ID = " + r.UserId;
                GenericGetDataResponse nameData = GetData(nameQuery);
                if (nameData.isSuccess && nameData.Data.Rows.Count > 0)
                    userName = nameData.Data.Rows[0][0].ToString();
            }

            resp.isSuccess = true;
            resp.Message = walletExists ? "Balance retrieved successfully" : "No wallet found. Please create a wallet first.";
            resp.UserId = r.UserId;
            resp.UserType = r.UserType;
            resp.UserName = userName;
            resp.Balance = balance;
            resp.LastUpdated = DateTime.Now;

            return resp;
        }

        // Get Wallet Transactions
        public GetWalletTransactionsResponse GetWalletTransactions(GetWalletTransactionsRequest r)
        {
            GetWalletTransactionsResponse resp = new GetWalletTransactionsResponse();
            resp.Transactions = new List<WalletTransactionItem>();

            string checkWalletQuery = "SELECT BALANCE FROM WALLETS WHERE USER_ID = " + r.UserId +
                                     " AND USER_TYPE = '" + r.UserType + "' AND STATUS = '" + GetSettingsValue("ACTIVE") + "'";

            GenericGetDataResponse walletData = GetData(checkWalletQuery);

            if (!walletData.isSuccess || walletData.Data.Rows.Count == 0)
            {
                resp.isSuccess = false;
                resp.Message = "No wallet found. Please create a wallet first.";
                resp.CurrentBalance = 0;
                return resp;
            }

            resp.CurrentBalance = Convert.ToDecimal(walletData.Data.Rows[0]["BALANCE"]);

            string query = "SELECT TRANSACTION_ID, TRANSACTION_TYPE, AMOUNT, BALANCE_BEFORE, " +
                           "BALANCE_AFTER, REMARKS, TRANSACTION_DATE " +
                           "FROM WALLET_TRANSACTIONS WHERE USER_ID = " + r.UserId +
                           " AND USER_TYPE = '" + r.UserType + "'";

            if (!string.IsNullOrEmpty(r.TransactionType))
            {
                query += " AND TRANSACTION_TYPE = '" + r.TransactionType + "'";
            }

            if (r.DateFrom.HasValue)
            {
                string dateFrom = r.DateFrom.Value.ToString("yyyy-MM-dd HH:mm:ss");
                query += " AND TRANSACTION_DATE >= '" + dateFrom + "'";
            }

            if (r.DateTo.HasValue)
            {
                string dateTo = r.DateTo.Value.ToString("yyyy-MM-dd HH:mm:ss");
                query += " AND TRANSACTION_DATE <= '" + dateTo + "'";
            }

            query += " ORDER BY TRANSACTION_DATE DESC";

            GenericGetDataResponse getData = GetData(query);

            if (getData.isSuccess && getData.Data.Rows.Count > 0)
            {
                foreach (DataRow dr in getData.Data.Rows)
                {
                    WalletTransactionItem item = new WalletTransactionItem();
                    item.TransactionId = Convert.ToInt32(dr["TRANSACTION_ID"]);
                    item.TransactionType = dr["TRANSACTION_TYPE"].ToString();
                    item.Amount = Convert.ToDecimal(dr["AMOUNT"]);
                    item.BalanceBefore = Convert.ToDecimal(dr["BALANCE_BEFORE"]);
                    item.BalanceAfter = Convert.ToDecimal(dr["BALANCE_AFTER"]);
                    item.Remarks = dr["REMARKS"]?.ToString() ?? "";
                    item.TransactionDate = Convert.ToDateTime(dr["TRANSACTION_DATE"]);

                    resp.Transactions.Add(item);
                }

                resp.TotalCount = resp.Transactions.Count;

                if (r.Page.HasValue && r.PageSize.HasValue && r.Page.Value > 0 && r.PageSize.Value > 0)
                {
                    resp.CurrentPage = r.Page.Value;
                    resp.PageSize = r.PageSize.Value;
                    resp.TotalPages = (int)Math.Ceiling((double)resp.TotalCount / r.PageSize.Value);

                    resp.Transactions = resp.Transactions
                        .Skip((r.Page.Value - 1) * r.PageSize.Value)
                        .Take(r.PageSize.Value)
                        .ToList();
                }

                resp.isSuccess = true;
                resp.Message = "Transactions retrieved successfully";
            }
            else
            {
                resp.isSuccess = true;
                resp.Message = "No transactions found";
                resp.TotalCount = 0;
            }

            return resp;
        }

    }
}
