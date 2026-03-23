using BaseCode.Models;
using BaseCode.Models.Requests;
using BaseCode.Models.Responses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vonage.Common;

namespace BaseCode.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BaseCodeController : Controller
    {
        private DBContext db;
        private readonly IWebHostEnvironment hostingEnvironment;
        private IHttpContextAccessor _IPAccess;


        private static readonly string[] Summaries = new[]
       {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private APILogResponse LogApi (string requestData, string responseData, string logTime)
        {
            string apiName = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;

            APILogRequest logRequest = new APILogRequest
            {
                ApiName = apiName,
                RequestData = requestData,
                ResponseData = responseData,
                LogTime = logTime
            };

            return db.LogAPI(logRequest);
        }

        public BaseCodeController(DBContext context, IWebHostEnvironment environment, IHttpContextAccessor accessor)
        {
            _IPAccess = accessor;
            db = context;
            hostingEnvironment = environment;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
        //Register Seller -------------------------------------------------
        [HttpPost("RegisterSeller")]
        public IActionResult RegisterSeller([FromBody] RegisterSellerRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            RegisterSellerResponse resp = new RegisterSellerResponse();
            string requestJson = JsonConvert.SerializeObject(r);
            if (string.IsNullOrEmpty(r.FirstName))
            {
                resp.Message = "Firstname required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.LastName))
            {
                resp.Message = "Lastname required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.Email))
            {
                resp.Message = "Email required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.ContactNo))
            {
                resp.Message = "Contact Number required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.UserName))
            {
                resp.Message = "Username required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.StoreName))
            {
                resp.Message = "Store Name required for Seller";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (db.CheckUserExists(r.UserName, r.Email, "SELLER"))
            {
                resp.isSuccess = false;
                resp.Message = "Username or Email already exists in Seller accounts";

                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.RegisterSeller(r);

            string responseJsonSuccess = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJsonSuccess, logTime);

            if (resp.isSuccess)
            {
                return Ok(resp);
            }
            else
            {
                return BadRequest(resp);
            }
        }
        //Register Buyer -------------------------------------------------
        [HttpPost("RegisterBuyer")]
        public IActionResult RegisterBuyer([FromBody] RegisterBuyerRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            RegisterBuyerResponse resp = new RegisterBuyerResponse();
            string requestJson = JsonConvert.SerializeObject(r);
            if (string.IsNullOrEmpty(r.FirstName))
            {
                resp.Message = "Firstname required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.LastName))
            {
                resp.Message = "Lastname required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.Email))
            {
                resp.Message = "Email required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.ContactNo))
            {
                resp.Message = "Contact Number required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.UserName))
            {
                resp.Message = "Username required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.PassPreference))
            {
                r.PassPreference = "email";
            }
            else
            {
                r.PassPreference = r.PassPreference.ToLower();
                if (r.PassPreference != "sms" && r.PassPreference != "email")
                {
                    r.PassPreference = "email";
                }
            }

            if (db.CheckUserExists(r.UserName, r.Email, "BUYER"))
            {
                resp.isSuccess = false;
                resp.Message = "Username or Email already exists in Buyer accounts";

                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.RegisterBuyer(r);

            string responseJsonSuccess = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJsonSuccess, logTime);

            if (resp.isSuccess)
            {
                return Ok(resp);
            }
            else
            {
                return BadRequest(resp);
            }
        }
        // --------------------------------------------------------

        // OTP Verification -----------------------------------------------------------
        [HttpPost("VerifyOTP")]
        public IActionResult VerifyOTP([FromBody] OTPVerificationRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            OTPVerificationResponse resp = new OTPVerificationResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.UserId <= 0)
            {
                resp.isSuccess = false;
                resp.Message = "User ID is required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.OTP) || r.OTP.Length != 8)
            {
                resp.isSuccess = false;
                resp.Message = "Valid 8-digit OTP is required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            resp = db.VerifyOTP(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
            {
                return Ok(resp);
            }
            else
            {
                return BadRequest(resp);
            }
        }
        // ----------------------------------------------------------------------------

        // Request New OTP ------------------------------------------------------------

        [HttpPost("RequestNewOTP")]
        public IActionResult RequestNewOTP([FromBody] RequestNewOTPRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            RequestNewOTPResponse resp = new RequestNewOTPResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (string.IsNullOrEmpty(r.Email))
            {
                resp.Message = "Please provide Email";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            resp = db.RequestNewOTP(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
            {
                return Ok(resp);
            }
            else
            {
                return BadRequest(resp);
            }
        }

        // ----------------------------------------------------------------------------

        // Login Seller ----------------------------------------------------------
        [HttpPost("LoginSeller")]
        public IActionResult LoginSeller([FromBody] LoginRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            LoginResponse resp = new LoginResponse();
            string requestJson = JsonConvert.SerializeObject(r);
            if (string.IsNullOrEmpty(r.UserName))
            {
                resp.isSuccess = false;
                resp.Message = "Please provide Username";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.Password))
            {
                resp.isSuccess = false;
                resp.Message = "Please provide Password";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (!db.CheckUserActive(r.UserName, "SELLER", db.GetSettingsValue("ACTIVE")))
            {
                resp.isSuccess = false;
                resp.Message = "Seller not found or inactive!";

                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.LoginSeller(r);

            string responseJsonSuccess = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJsonSuccess, logTime);

            if (resp.isSuccess)
            {
                return Ok(resp);
            }
            else
            {
                return BadRequest(resp);
            }
        }

        // Login Buyer -----------------------------------------------------------
        [HttpPost("LoginBuyer")]
        public IActionResult LoginBuyer([FromBody] LoginRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            LoginResponse resp = new LoginResponse();
            string requestJson = JsonConvert.SerializeObject(r);
            if (string.IsNullOrEmpty(r.UserName))
            {
                resp.isSuccess = false;
                resp.Message = "Please provide Username";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.Password))
            {
                resp.isSuccess = false;
                resp.Message = "Please provide Password";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (!db.CheckUserActive(r.UserName, "BUYER", db.GetSettingsValue("ACTIVE")))
            {
                resp.isSuccess = false;
                resp.Message = "Buyer not found or inactive!";

                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.LoginBuyer(r);

            string responseJsonSuccess = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJsonSuccess, logTime);

            if (resp.isSuccess)
            {
                return Ok(resp);
            }
            else
            {
                return BadRequest(resp);
            }
        }
        // -------------------------------------------------------------

        // Logout Seller ---------------------------------------------------
        [HttpPost("LogoutSeller")]
        public IActionResult LogoutSeller([FromBody] LogoutRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            LogoutResponse resp = new LogoutResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            int userId = db.GetUserIdFromSession(r.SessionKey, "SELLER");
            if (userId <= 0)
            {
                resp.isSuccess = false;
                resp.Message = "No active seller session found";

                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.LogoutSeller(r);

            string responseJsonSuccess = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJsonSuccess, logTime);

            if (resp.isSuccess)
            {
                return Ok(resp);
            }
            else
            {
                return BadRequest(resp);
            }
        }

        // Logout Buyer ---------------------------------------------------
        [HttpPost("LogoutBuyer")]
        public IActionResult LogoutBuyer([FromBody] LogoutRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            LogoutResponse resp = new LogoutResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            int userId = db.GetUserIdFromSession(r.SessionKey, "BUYER");
            if (userId <= 0)
            {
                resp.isSuccess = false;
                resp.Message = "No active buyer session found";

                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.LogoutBuyer(r);

            string responseJsonSuccess = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJsonSuccess, logTime);

            if (resp.isSuccess)
            {
                return Ok(resp);
            }
            else
            {
                return BadRequest(resp);
            }
        }
        // ----------------------------------------------------------------
        // Change Password Seller ------------------------------------------
        [HttpPost("ChangePasswordSeller")]
        public IActionResult ChangePasswordSeller([FromBody] ChangePasswordRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            ChangePasswordResponse resp = new ChangePasswordResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.UserId <= 0)
            {
                resp.Message = "Please provide User ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide session key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.CurrentPassword))
            {
                resp.Message = "Please provide current Password";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.NewPassword))
            {
                resp.Message = "Please provide new Password";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (r.NewPassword.Length < 8)
            {
                resp.isSuccess = false;
                resp.Message = "Password must be at least 8 characters long";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "SELLER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid seller session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            int sessionUserId = db.GetUserIdFromSession(r.SessionKey, "SELLER");
            if (sessionUserId <= 0)
            {
                resp.isSuccess = false;
                resp.Message = "Please login";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            if (sessionUserId != r.UserId)
            {
                resp.isSuccess = false;
                resp.Message = "User ID does not match session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.ChangePasswordSeller(r);

            string responseJsonSuccess = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJsonSuccess, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // Change Password Buyer ------------------------------------------
        [HttpPost("ChangePasswordBuyer")]
        public IActionResult ChangePasswordBuyer([FromBody] ChangePasswordRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            ChangePasswordResponse resp = new ChangePasswordResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.UserId <= 0)
            {
                resp.Message = "Please provide User ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide session key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.CurrentPassword))
            {
                resp.Message = "Please provide current Password";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.NewPassword))
            {
                resp.Message = "Please provide new Password";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (r.NewPassword.Length < 8)
            {
                resp.isSuccess = false;
                resp.Message = "Password must be at least 8 characters long";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "BUYER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid buyer session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            int sessionUserId = db.GetUserIdFromSession(r.SessionKey, "BUYER");
            if (sessionUserId <= 0)
            {
                resp.isSuccess = false;
                resp.Message = "Please login";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            if (sessionUserId != r.UserId)
            {
                resp.isSuccess = false;
                resp.Message = "User ID does not match session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.ChangePasswordBuyer(r);

            string responseJsonSuccess = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJsonSuccess, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }
        // -----------------------------------------------
        // Forgot Password Seller ------------------------------
        [HttpPost("ForgotPasswordSeller")]
        public IActionResult ForgotPasswordSeller([FromBody] ForgotPasswordRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            ForgotPasswordResponse resp = new ForgotPasswordResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (string.IsNullOrEmpty(r.Email))
            {
                resp.Message = "Please provide Email";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            string query = "SELECT SELLER_ID, SELLER_FIRST_NAME, SELLER_USERNAME, SELLER_EMAIL FROM SELLERS WHERE SELLER_EMAIL = '" + r.Email.Replace("'", "''") + "' AND SELLER_STATUS = '" + activeStatus + "'";

            GenericGetDataResponse getUserData = db.GetData(query);

            if (getUserData.Data.Rows.Count == 0)
            {
                resp.isSuccess = false;
                resp.Message = "Seller not found or inactive!";

                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.ForgotPasswordSeller(r, getUserData);

            string responseJsonSuccess = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJsonSuccess, logTime);

            if (resp.isSuccess)
            {
                return Ok(resp);
            }
            else
            {
                return BadRequest(resp);
            }
        }

        // Forgot Password Buyer ------------------------------
        [HttpPost("ForgotPasswordBuyer")]
        public IActionResult ForgotPasswordBuyer([FromBody] ForgotPasswordRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            ForgotPasswordResponse resp = new ForgotPasswordResponse();
            string requestJson = JsonConvert.SerializeObject(r);
            if (string.IsNullOrEmpty(r.Email))
            {
                resp.Message = "Please provide Email";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            string query = "SELECT BUYER_ID, BUYER_FIRST_NAME, BUYER_USERNAME, BUYER_EMAIL, BUYER_CONTACT_NO FROM BUYERS WHERE BUYER_EMAIL = '" + r.Email.Replace("'", "''") + "' AND BUYER_STATUS = '" + activeStatus + "'";

            GenericGetDataResponse getUserData = db.GetData(query);

            if (getUserData.Data.Rows.Count == 0)
            {
                resp.isSuccess = false;
                resp.Message = "Buyer not found or inactive!";

                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.ForgotPasswordBuyer(r, getUserData);

            string responseJsonSuccess = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJsonSuccess, logTime);

            if (resp.isSuccess)
            {
                return Ok(resp);
            }
            else
            {
                return BadRequest(resp);
            }
        }
        // -----------------------------------------------

        // Delete Seller --------------------------------------------------------------
        [HttpPost("DeleteSeller")]
        public IActionResult DeleteSeller([FromBody] DeleteAccountRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            DeleteAccountResponse resp = new DeleteAccountResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.UserId <= 0)
            {
                resp.Message = "Please provide User ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "SELLER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid seller session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            int sessionUserId = db.GetUserIdFromSession(r.SessionKey, "SELLER");
            if (sessionUserId != r.UserId)
            {
                resp.isSuccess = false;
                resp.Message = "User ID does not match session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.DeleteSeller(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
            {
                return Ok(resp);
            }
            else
            {
                return BadRequest(resp);
            }
        }

        // Delete Buyer -----------------------------------------------------------------
        [HttpPost("DeleteBuyer")]
        public IActionResult DeleteBuyer([FromBody] DeleteAccountRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            DeleteAccountResponse resp = new DeleteAccountResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.UserId <= 0)
            {
                resp.Message = "Please provide User ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "BUYER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid buyer session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            int sessionUserId = db.GetUserIdFromSession(r.SessionKey, "BUYER");
            if (sessionUserId != r.UserId)
            {
                resp.isSuccess = false;
                resp.Message = "User ID does not match session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.DeleteBuyer(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
            {
                return Ok(resp);
            }
            else
            {
                return BadRequest(resp);
            }
        }
        // ---------------------------------------------------------------------------------------------------------

        // Get Seller Profile --------------------------------------------------------------------------------------
        [HttpPost("GetSellerProfile")]
        public IActionResult GetSellerProfile([FromBody] GetSellerProfileRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");
            GetSellerProfileResponse resp = new GetSellerProfileResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.SellerId <= 0)
            {
                resp.Message = "Please provide Seller ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "SELLER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid seller session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            int sessionUserId = db.GetUserIdFromSession(r.SessionKey, "SELLER");
            if (sessionUserId != r.SellerId)
            {
                resp.isSuccess = false;
                resp.Message = "Session does not match seller ID";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.GetSellerProfile(r);
            string responseJsonSuccess = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJsonSuccess, logTime);

            if (resp.isSuccess)
            {
                return Ok(resp);
            }
            else
            {
                return BadRequest(resp);
            }
        }
        // ----------------------------------------------------------------------------------------

        // Get Buyer Profile -----------------------------------------------------------------------
        [HttpPost("GetBuyerProfile")]
        public IActionResult GetBuyerProfile([FromBody] GetBuyerProfileRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");
            GetBuyerProfileResponse resp = new GetBuyerProfileResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.BuyerId <= 0)
            {
                resp.Message = "Please provide Buyer ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "BUYER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid buyer session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            int sessionUserId = db.GetUserIdFromSession(r.SessionKey, "BUYER");
            if (sessionUserId != r.BuyerId)
            {
                resp.isSuccess = false;
                resp.Message = "Session does not match buyer ID";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.GetBuyerProfile(r);
            string responseJsonSuccess = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJsonSuccess, logTime);

            if (resp.isSuccess)
            {
                return Ok(resp);
            }
            else
            {
                return BadRequest(resp);
            }
        }
        // ------------------------------------------------------------------------------------------

        // Update Seller Profile --------------------------------------------------------------------

        [HttpPost("UpdateSellerProfile")]
        public IActionResult UpdateSellerProfile([FromBody] UpdateSellerProfileRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");
            UpdateSellerProfileResponse resp = new UpdateSellerProfileResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.SellerId <= 0)
            {
                resp.Message = "Please provide Seller ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.FirstName) && string.IsNullOrEmpty(r.LastName) &&
                string.IsNullOrEmpty(r.StoreName) && string.IsNullOrEmpty(r.UserName))
            {
                resp.Message = "Please provide at least one field to update";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "SELLER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid seller session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            int sessionUserId = db.GetUserIdFromSession(r.SessionKey, "SELLER");
            if (sessionUserId != r.SellerId)
            {
                resp.isSuccess = false;
                resp.Message = "Session does not match seller ID";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            if (!db.CheckUserActiveByID(r.SellerId, "SELLER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Seller not found or inactive";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            if (!string.IsNullOrEmpty(r.UserName))
            {
                string checkCurrentQuery = "SELECT SELLER_USERNAME FROM SELLERS WHERE SELLER_ID = " + r.SellerId;
                GenericGetDataResponse currentData = db.GetData(checkCurrentQuery);
                string currentUsername = currentData.Data.Rows[0][0].ToString();

                if (r.UserName != currentUsername)
                {
                    if (db.CheckUserExists(r.UserName, "", "SELLER"))
                    {
                        resp.isSuccess = false;
                        resp.Message = "Username already exists";
                        string responseJsonError = JsonConvert.SerializeObject(resp);
                        LogApi(requestJson, responseJsonError, logTime);
                        return BadRequest(resp);
                    }
                }
            }

            resp = db.UpdateSellerProfile(r);
            string responseJsonSuccess = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJsonSuccess, logTime);

            if (resp.isSuccess)
            {
                return Ok(resp);
            }
            else
            {
                return BadRequest(resp);
            }
        }
        // -----------------------------------------------------------------------------------------

        // Update Buyer Profile --------------------------------------------------------------------

        [HttpPost("UpdateBuyerProfile")]
        public IActionResult UpdateBuyerProfile([FromBody] UpdateBuyerProfileRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");
            UpdateBuyerProfileResponse resp = new UpdateBuyerProfileResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.BuyerId <= 0)
            {
                resp.Message = "Please provide Buyer ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.FirstName) && string.IsNullOrEmpty(r.LastName) &&
                string.IsNullOrEmpty(r.UserName))
            {
                resp.Message = "Please provide at least one field to update";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "BUYER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid buyer session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            int sessionUserId = db.GetUserIdFromSession(r.SessionKey, "BUYER");
            if (sessionUserId != r.BuyerId)
            {
                resp.isSuccess = false;
                resp.Message = "Session does not match buyer ID";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            if (!db.CheckUserActiveByID(r.BuyerId, "BUYER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Buyer not found or inactive";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }
            if (!string.IsNullOrEmpty(r.UserName))
            {
                string checkCurrentQuery = "SELECT BUYER_USERNAME FROM BUYERS WHERE BUYER_ID = " + r.BuyerId;
                GenericGetDataResponse currentData = db.GetData(checkCurrentQuery);
                string currentUsername = currentData.Data.Rows[0][0].ToString();
                if (r.UserName != currentUsername)
                {
                    if (db.CheckUserExists(r.UserName, "", "BUYER"))
                    {
                        resp.isSuccess = false;
                        resp.Message = "Username already exists";
                        string responseJsonError = JsonConvert.SerializeObject(resp);
                        LogApi(requestJson, responseJsonError, logTime);
                        return BadRequest(resp);
                    }
                }
            }

            resp = db.UpdateBuyerProfile(r);
            string responseJsonSuccess = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJsonSuccess, logTime);

            if (resp.isSuccess)
            {
                return Ok(resp);
            }
            else
            {
                return BadRequest(resp);
            }
        }

        // -----------------------------------------------------------------------------------------














        // Add Product (Seller) ------------------------------------------------------------------------------------
        [HttpPost("AddProduct")]
        public IActionResult AddProduct([FromBody] AddProductRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            AddProductResponse resp = new AddProductResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.SellerId <= 0)
            {
                resp.Message = "Please provide Seller ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.ProductName))
            {
                resp.Message = "Product Name is required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.ProductDescription))
            {
                resp.Message = "Product Description is required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (r.Price <= 0)
            {
                resp.Message = "Valid Product Price is required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (r.Quantity <= 0)
            {
                resp.Message = "Valid Product Quantity is required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "SELLER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid seller session. Please login again.";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            int sessionSellerId = db.GetUserIdFromSession(r.SessionKey, "SELLER");
            if (sessionSellerId != r.SellerId)
            {
                resp.isSuccess = false;
                resp.Message = "Session does not match seller ID";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            if (!db.CheckUserActiveByID(r.SellerId, "SELLER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Seller not found or inactive";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.AddProduct(r);

            string responseJsonSuccess = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJsonSuccess, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // Update Product (Seller) ------------------------------------------------------------------------------------
        [HttpPost("UpdateProduct")]
        public IActionResult UpdateProduct([FromBody] UpdateProductRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            UpdateProductResponse resp = new UpdateProductResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.SellerId <= 0)
            {
                resp.Message = "Please provide Seller ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (r.ProductId <= 0)
            {
                resp.Message = "Please provide Product ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.ProductName) &&
                string.IsNullOrEmpty(r.ProductDescription) &&
                !r.Price.HasValue &&
                !r.Quantity.HasValue &&
                string.IsNullOrEmpty(r.Category) &&
                string.IsNullOrEmpty(r.Brand))
            {
                resp.Message = "Please provide at least one field to update";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (r.Price.HasValue && r.Price.Value <= 0)
            {
                resp.Message = "Price must be greater than 0";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (r.Quantity.HasValue && r.Quantity.Value < 0)
            {
                resp.Message = "Quantity cannot be negative";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "SELLER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid seller session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            int sessionSellerId = db.GetUserIdFromSession(r.SessionKey, "SELLER");
            if (sessionSellerId != r.SellerId)
            {
                resp.isSuccess = false;
                resp.Message = "Session does not match seller ID";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            if (!db.CheckProductBelongsToSeller(r.ProductId, r.SellerId))
            {
                resp.isSuccess = false;
                resp.Message = "Product not found or does not belong to this seller";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.UpdateProduct(r);

            string responseJsonSuccess = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJsonSuccess, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // Products Listing (Seller) -------------------------------------------------------------------------------
        [HttpPost("GetProducts")]
        public IActionResult GetProducts([FromBody] GetProductsRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            GetProductsResponse resp = new GetProductsResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            bool isAuthenticated = !string.IsNullOrEmpty(r.SessionKey) && r.UserId.HasValue && !string.IsNullOrEmpty(r.UserType);
            string activeStatus = db.GetSettingsValue("ACTIVE");

            if (isAuthenticated)
            {
                bool isValidSession = r.UserType.ToUpper() == "SELLER"
                    ? db.CheckSessionActive(r.SessionKey, "SELLER", activeStatus)
                    : db.CheckSessionActive(r.SessionKey, "BUYER", activeStatus);

                if (!isValidSession)
                {
                    resp.isSuccess = false;
                    resp.Message = "Invalid session. Please login again.";
                    string responseJsonError = JsonConvert.SerializeObject(resp);
                    LogApi(requestJson, responseJsonError, logTime);
                    return Unauthorized(resp);
                }
                int sessionUserId = db.GetUserIdFromSession(r.SessionKey, r.UserType);
                if (sessionUserId != r.UserId.Value)
                {
                    resp.isSuccess = false;
                    resp.Message = "User ID does not match session";
                    string responseJsonError = JsonConvert.SerializeObject(resp);
                    LogApi(requestJson, responseJsonError, logTime);
                    return Unauthorized(resp);
                }
                if (r.SellerId.HasValue && r.SellerId.Value > 0)
                {
                    if (r.UserType.ToUpper() == "SELLER" && r.SellerId.Value != r.UserId.Value)
                    {
                        resp.isSuccess = false;
                        resp.Message = "Sellers can only view their own products";
                        string responseJsonError = JsonConvert.SerializeObject(resp);
                        LogApi(requestJson, responseJsonError, logTime);
                        return Unauthorized(resp);
                    }
                }
            }
            else
            {
                if (r.SellerId.HasValue && r.SellerId.Value > 0)
                {
                    resp.isSuccess = false;
                    resp.Message = "Authentication required to view seller-specific products";
                    string responseJsonError = JsonConvert.SerializeObject(resp);
                    LogApi(requestJson, responseJsonError, logTime);
                    return Unauthorized(resp);
                }
                if (r.Status != "ACTIVE")
                {
                    r.Status = "ACTIVE";
                }
            }

            if (r.SellerId.HasValue && r.SellerId.Value > 0)
            {
                string sellerQuery = "SELECT SELLER_STORE_NAME, SELLER_FIRST_NAME, SELLER_LAST_NAME FROM SELLERS WHERE SELLER_ID = " + r.SellerId.Value;
                GenericGetDataResponse sellerData = db.GetData(sellerQuery);
                if (sellerData.Data.Rows.Count > 0)
                {
                    resp.SellerId = r.SellerId.Value;
                    resp.SellerName = sellerData.Data.Rows[0]["SELLER_STORE_NAME"]?.ToString()
                        ?? sellerData.Data.Rows[0]["SELLER_FIRST_NAME"] + " " + sellerData.Data.Rows[0]["SELLER_LAST_NAME"];
                }
            }

            resp = db.GetProducts(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }
        // ----------------------------------------------------------------------------------------

        // Get Products By Category ---------------------------------------------------------------
        [HttpPost("GetProductsByCategory")]
        public IActionResult GetProductsByCategory([FromBody] GetProductByCategoryRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            GetProductByCategoryResponse resp = new GetProductByCategoryResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.CategoryId <= 0)
            {
                resp.Message = "Please provide Category ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            resp = db.GetProductsByCategory(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
            {
                return Ok(resp);
            }
            else
            {
                return BadRequest(resp);
            }
        }

        // ----------------------------------------------------------------------------------------

        // Delete Product -------------------------------------------------------------------------
        [HttpPost("DeleteProduct")]
        public IActionResult DeleteProduct([FromBody] DeleteProductRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            DeleteProductResponse resp = new DeleteProductResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.SellerId <= 0)
            {
                resp.Message = "Please provide Seller ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (r.ProductId <= 0)
            {
                resp.Message = "Please provide Product ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "SELLER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid seller session. Please login again.";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            int sessionSellerId = db.GetUserIdFromSession(r.SessionKey, "SELLER");
            if (sessionSellerId != r.SellerId)
            {
                resp.isSuccess = false;
                resp.Message = "Session does not match seller ID";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            if (!db.CheckUserActiveByID(r.SellerId, "SELLER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Seller not found or inactive";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.DeleteProduct(r);

            string responseJsonSuccess = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJsonSuccess, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // Add Review
        [HttpPost("AddReview")]
        public IActionResult AddReview([FromBody] AddReviewRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");
            string requestJson = JsonConvert.SerializeObject(r);
            AddReviewResponse resp = new AddReviewResponse();

            if (r.BuyerId <= 0)
            {
                resp.Message = "Please provide Buyer ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (r.ProductId <= 0)
            {
                resp.Message = "Please provide Product ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (r.Rating < 1 || r.Rating > 5)
            {
                resp.Message = "Rating must be between 1 and 5";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "BUYER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid buyer session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }

            int sessionBuyerId = db.GetUserIdFromSession(r.SessionKey, "BUYER");
            if (sessionBuyerId != r.BuyerId)
            {
                resp.isSuccess = false;
                resp.Message = "Buyer ID does not match session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }

            resp = db.AddReview(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }


        // Get Reviews
        [HttpPost("GetProductReviews")]
        public IActionResult GetProductReviews([FromBody] GetProductReviewsRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.ProductId <= 0)
            {
                GetProductReviewsResponse errorResp = new GetProductReviewsResponse();
                errorResp.Message = "Please provide Product ID";
                LogApi(requestJson, errorResp.Message, logTime);
                return BadRequest(errorResp);
            }

            GetProductReviewsResponse resp = db.GetProductReviews(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // Add Comments
        [HttpPost("AddComment")]
        public IActionResult AddComment([FromBody] AddCommentRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");
            string requestJson = JsonConvert.SerializeObject(r);
            AddCommentResponse resp = new AddCommentResponse();

            if (r.UserId <= 0)
            {
                resp.Message = "Please provide User ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.UserType) || (r.UserType.ToUpper() != "BUYER" && r.UserType.ToUpper() != "SELLER"))
            {
                resp.Message = "Valid User Type (BUYER or SELLER) is required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (r.ProductId <= 0)
            {
                resp.Message = "Please provide Product ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.CommentText))
            {
                resp.Message = "Comment text is required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, r.UserType, activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }

            int sessionUserId = db.GetUserIdFromSession(r.SessionKey, r.UserType);
            if (sessionUserId != r.UserId)
            {
                resp.isSuccess = false;
                resp.Message = "User ID does not match session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }

            resp = db.AddComment(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }


        // Get Product Comments
        [HttpPost("GetProductComments")]
        public IActionResult GetProductComments([FromBody] GetProductCommentsRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.ProductId <= 0)
            {
                GetProductCommentsResponse errorResp = new GetProductCommentsResponse();
                errorResp.Message = "Please provide Product ID";
                LogApi(requestJson, errorResp.Message, logTime);
                return BadRequest(errorResp);
            }

            GetProductCommentsResponse resp = db.GetProductComments(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }
        // ---------------------------------------------------------------------------------------------------------








        // CART ----------------------------------------------------------------------------------------------------------
        // Add To Cart ----------------------------------------------------------------------------
        [HttpPost("AddToCart")]
        public IActionResult AddToCart([FromBody] AddToCartRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            AddToCartResponse resp = new AddToCartResponse();
            string requestJson = JsonConvert.SerializeObject(r);
            if (r.BuyerId <= 0)
            {
                resp.Message = "Please provide Buyer ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (r.ProductId <= 0)
            {
                resp.Message = "Please provide Product ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (r.Quantity <= 0)
            {
                resp.Message = "Quantity must be greater than 0";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "BUYER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid buyer session. Please login again.";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            int sessionBuyerId = db.GetUserIdFromSession(r.SessionKey, "BUYER");
            if (sessionBuyerId != r.BuyerId)
            {
                resp.isSuccess = false;
                resp.Message = "Session does not match buyer ID";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            if (!db.CheckUserActiveByID(r.BuyerId, "BUYER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Buyer not found or inactive";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.AddToCart(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // Get Cart
        [HttpPost("GetCart")]
        public IActionResult GetCart([FromBody] GetCartRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            GetCartResponse resp = new GetCartResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.BuyerId <= 0)
            {
                resp.Message = "Please provide Buyer ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "BUYER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid buyer session. Please login again.";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            int sessionBuyerId = db.GetUserIdFromSession(r.SessionKey, "BUYER");
            if (sessionBuyerId != r.BuyerId)
            {
                resp.isSuccess = false;
                resp.Message = "Session does not match buyer ID";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.GetCart(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // Update Cart Item
        [HttpPost("UpdateCartItem")]
        public IActionResult UpdateCartItem([FromBody] UpdateCartItemRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            UpdateCartItemResponse resp = new UpdateCartItemResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.BuyerId <= 0)
            {
                resp.Message = "Please provide Buyer ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (r.CartId <= 0)
            {
                resp.Message = "Please provide Cart ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (r.NewQuantity < 0)
            {
                resp.Message = "Quantity cannot be negative";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "BUYER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid buyer session. Please login again.";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            int sessionBuyerId = db.GetUserIdFromSession(r.SessionKey, "BUYER");
            if (sessionBuyerId != r.BuyerId)
            {
                resp.isSuccess = false;
                resp.Message = "Session does not match buyer ID";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.UpdateCartItem(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // Remove from Cart
        [HttpPost("RemoveFromCart")]
        public IActionResult RemoveFromCart([FromBody] RemoveFromCartRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            RemoveFromCartResponse resp = new RemoveFromCartResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.BuyerId <= 0)
            {
                resp.Message = "Please provide Buyer ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (r.CartId <= 0)
            {
                resp.Message = "Please provide Cart ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "BUYER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid buyer session. Please login again.";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            int sessionBuyerId = db.GetUserIdFromSession(r.SessionKey, "BUYER");
            if (sessionBuyerId != r.BuyerId)
            {
                resp.isSuccess = false;
                resp.Message = "Session does not match buyer ID";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.RemoveFromCart(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // Clear Cart
        [HttpPost("ClearCart")]
        public IActionResult ClearCart([FromBody] ClearCartRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            ClearCartResponse resp = new ClearCartResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.BuyerId <= 0)
            {
                resp.Message = "Please provide Buyer ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.SessionKey))
            {
               resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "BUYER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid buyer session. Please login again.";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            int sessionBuyerId = db.GetUserIdFromSession(r.SessionKey, "BUYER");
            if (sessionBuyerId != r.BuyerId)
            {
                resp.isSuccess = false;
                resp.Message = "Session does not match buyer ID";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return BadRequest(resp);
            }

            resp = db.ClearCart(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // Create Voucher
        [HttpPost("CreateVoucher")]
        public IActionResult CreateVoucher([FromBody] CreateVoucherRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            CreateVoucherResponse resp = new CreateVoucherResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (string.IsNullOrEmpty(r.SessionKey))
            {                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "SELLER", activeStatus) &&
                !db.CheckSessionActive(r.SessionKey, "BUYER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid session. Please login again.";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }
            if (string.IsNullOrEmpty(r.VoucherCode))
            {
                resp.Message = "Voucher code is required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.VoucherName))
            {
                resp.Message = "Voucher name is required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (r.DiscountAmount <= 0)
            {
                resp.Message = "Discount amount must be greater than 0";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (r.MinimumSpend < 0)
            {
                resp.Message = "Minimum spend cannot be negative";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (r.ValidFrom >= r.ValidTo)
            {
                resp.Message = "Valid To date must be after Valid From date";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (r.MaxUses <= 0)
            {
                resp.Message = "Maximum uses must be greater than 0";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            resp = db.CreateVoucher(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // Delete Voucher
        [HttpPost("DeleteVoucher")]
        public IActionResult DeleteVoucher([FromBody] DeleteVoucherRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            DeleteVoucherResponse resp = new DeleteVoucherResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.VoucherId <= 0)
            {
                resp.Message = "Please provide Voucher ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, "SELLER", activeStatus) &&
                !db.CheckSessionActive(r.SessionKey, "BUYER", activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid session. Please login again.";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }

            resp = db.DeleteVoucher(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // Get Vouchers
        [HttpPost("GetVouchers")]
        public IActionResult GetVouchers([FromBody] GetVouchersRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            GetVouchersResponse resp = new GetVouchersResponse();
            string requestJson = JsonConvert.SerializeObject(r);
            if (r.SellerId.HasValue && r.SellerId.Value > 0)
            {
                if (string.IsNullOrEmpty(r.SessionKey))
                {
                    resp.Message = "Please provide Session Key";
                    LogApi(requestJson, resp.Message, logTime);
                    return BadRequest(resp);
                }

                string activeStatus = db.GetSettingsValue("ACTIVE");
                if (!db.CheckSessionActive(r.SessionKey, "SELLER", activeStatus))
                {
                    resp.isSuccess = false;
                    resp.Message = "Invalid seller session";
                    string responseJsonError = JsonConvert.SerializeObject(resp);
                    LogApi(requestJson, responseJsonError, logTime);
                    return Unauthorized(resp);
                }

                int sessionSellerId = db.GetUserIdFromSession(r.SessionKey, "SELLER");
                if (sessionSellerId != r.SellerId.Value)
                {
                    resp.isSuccess = false;
                    resp.Message = "Seller ID does not match session";
                    string responseJsonError = JsonConvert.SerializeObject(resp);
                    LogApi(requestJson, responseJsonError, logTime);
                    return Unauthorized(resp);
                }
            }

            resp = db.GetVouchers(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // Get Active Vouchers
        [HttpGet("GetActiveVouchers")]
        public IActionResult GetActiveVouchers()
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            GetVouchersResponse resp = db.GetActiveVouchers();

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi("{}", responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("ValidateVoucher")]
        public IActionResult ValidateVoucher([FromBody] ValidateVoucherRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            ValidateVoucherResponse resp = new ValidateVoucherResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (string.IsNullOrEmpty(r.VoucherCode))
            {
                resp.Message = "Please provide voucher code";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (r.CartTotal <= 0)
            {
                resp.Message = "Cart total must be greater than 0";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            resp = db.ValidateVoucher(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            return Ok(resp);
        }

        // Apply Voucher
        [HttpPost("ApplyVoucher")]
        public IActionResult ApplyVoucher([FromBody] ApplyVoucherRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            ApplyVoucherResponse resp = new ApplyVoucherResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.UserId <= 0)
            {
                resp.Message = "Please provide User ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.VoucherCode))
            {
                resp.Message = "Please provide voucher code";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (r.CartTotal <= 0)
            {
                resp.Message = "Cart total must be greater than 0";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.UserType))
            {
                r.UserType = "BUYER";
            }


            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, r.UserType, activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid session. Please login again.";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }
            int sessionUserId = db.GetUserIdFromSession(r.SessionKey, r.UserType);
            if (sessionUserId != r.UserId)
            {
                resp.isSuccess = false;
                resp.Message = "User ID does not match session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }

            resp = db.ApplyVoucher(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // ----------------------------------------------------------------------------------------

        // E-Wallet ----------------------------------------------------------------------------------------------------------

        // Request Wallet Code
        [HttpPost("RequestWalletCode")]
        public IActionResult RequestWalletCode([FromBody] RequestWalletCodeRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            RequestWalletCodeResponse resp = new RequestWalletCodeResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.UserId <= 0)
            {
                resp.Message = "Please provide User ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.UserType) || (r.UserType.ToUpper() != "SELLER" && r.UserType.ToUpper() != "BUYER"))
            {
                resp.Message = "Valid User Type (SELLER or BUYER) is required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.Email))
            {
                resp.Message = "Please provide Email";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, r.UserType, activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid session. Please login again.";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }

            int sessionUserId = db.GetUserIdFromSession(r.SessionKey, r.UserType);
            if (sessionUserId != r.UserId)
            {
                resp.isSuccess = false;
                resp.Message = "User ID does not match session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }

            resp = db.RequestWalletCode(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // Create Wallet 
        [HttpPost("CreateWallet")]
        public IActionResult CreateWallet([FromBody] CreateWalletRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            CreateWalletResponse resp = new CreateWalletResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.UserId <= 0)
            {
                resp.Message = "Please provide User ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.UserType) || (r.UserType.ToUpper() != "SELLER" && r.UserType.ToUpper() != "BUYER"))
            {
                resp.Message = "Valid User Type (SELLER or BUYER) is required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.Email))
            {
                resp.Message = "Please provide Email";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.VerificationCode))
            {
                resp.Message = "Please provide Verification Code";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, r.UserType, activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid session. Please login again.";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }

            int sessionUserId = db.GetUserIdFromSession(r.SessionKey, r.UserType);
            if (sessionUserId != r.UserId)
            {
                resp.isSuccess = false;
                resp.Message = "User ID does not match session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }

            resp = db.CreateWallet(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // Deposit to Wallet
        [HttpPost("DepositToWallet")]
        public IActionResult DepositToWallet([FromBody] WalletDepositRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            WalletDepositResponse resp = new WalletDepositResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.UserId <= 0)
            {
                resp.Message = "Please provide User ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.UserType) || (r.UserType.ToUpper() != "SELLER" && r.UserType.ToUpper() != "BUYER"))
            {
                resp.Message = "Valid User Type (SELLER or BUYER) is required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (r.Amount <= 0)
            {
                resp.Message = "Deposit amount must be greater than 0";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, r.UserType, activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid session. Please login again.";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }

            int sessionUserId = db.GetUserIdFromSession(r.SessionKey, r.UserType);
            if (sessionUserId != r.UserId)
            {
                resp.isSuccess = false;
                resp.Message = "User ID does not match session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }

            resp = db.DepositToWallet(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // Withdraw from Wallet
        [HttpPost("WithdrawFromWallet")]
        public IActionResult WithdrawFromWallet([FromBody] WalletWithdrawRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            WalletWithdrawResponse resp = new WalletWithdrawResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.UserId <= 0)
            {
                resp.Message = "Please provide User ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.UserType) || (r.UserType.ToUpper() != "SELLER" && r.UserType.ToUpper() != "BUYER"))
            {
                resp.Message = "Valid User Type (SELLER or BUYER) is required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (r.Amount <= 0)
            {
                resp.Message = "Withdrawal amount must be greater than 0";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, r.UserType, activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid session. Please login again.";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }

            int sessionUserId = db.GetUserIdFromSession(r.SessionKey, r.UserType);
            if (sessionUserId != r.UserId)
            {
                resp.isSuccess = false;
                resp.Message = "User ID does not match session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }

            resp = db.WithdrawFromWallet(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // Get Wallet Balance
        [HttpPost("GetWalletBalance")]
        public IActionResult GetWalletBalance([FromBody] GetWalletBalanceRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            GetWalletBalanceResponse resp = new GetWalletBalanceResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.UserId <= 0)
            {
                resp.Message = "Please provide User ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.UserType) || (r.UserType.ToUpper() != "SELLER" && r.UserType.ToUpper() != "BUYER"))
            {
                resp.Message = "Valid User Type (SELLER or BUYER) is required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, r.UserType, activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid session. Please login again.";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }

            int sessionUserId = db.GetUserIdFromSession(r.SessionKey, r.UserType);
            if (sessionUserId != r.UserId)
            {
                resp.isSuccess = false;
                resp.Message = "User ID does not match session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }

            resp = db.GetWalletBalance(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        // Get Wallet Transactions
        [HttpPost("GetWalletTransactions")]
        public IActionResult GetWalletTransactions([FromBody] GetWalletTransactionsRequest r)
        {
            DateTime apiCallTime = DateTime.Now;
            string logTime = apiCallTime.ToString("yyyy-MM-dd HH:mm:ss");

            GetWalletTransactionsResponse resp = new GetWalletTransactionsResponse();
            string requestJson = JsonConvert.SerializeObject(r);

            if (r.UserId <= 0)
            {
                resp.Message = "Please provide User ID";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.SessionKey))
            {
                resp.Message = "Please provide Session Key";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            if (string.IsNullOrEmpty(r.UserType) || (r.UserType.ToUpper() != "SELLER" && r.UserType.ToUpper() != "BUYER"))
            {
                resp.Message = "Valid User Type (SELLER or BUYER) is required";
                LogApi(requestJson, resp.Message, logTime);
                return BadRequest(resp);
            }

            string activeStatus = db.GetSettingsValue("ACTIVE");
            if (!db.CheckSessionActive(r.SessionKey, r.UserType, activeStatus))
            {
                resp.isSuccess = false;
                resp.Message = "Invalid session. Please login again.";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }

            int sessionUserId = db.GetUserIdFromSession(r.SessionKey, r.UserType);
            if (sessionUserId != r.UserId)
            {
                resp.isSuccess = false;
                resp.Message = "User ID does not match session";
                string responseJsonError = JsonConvert.SerializeObject(resp);
                LogApi(requestJson, responseJsonError, logTime);
                return Unauthorized(resp);
            }

            resp = db.GetWalletTransactions(r);

            string responseJson = JsonConvert.SerializeObject(resp);
            LogApi(requestJson, responseJson, logTime);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }
    }
}
