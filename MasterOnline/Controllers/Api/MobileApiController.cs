using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using MasterOnline.Models;
using MasterOnline.Models.Api;
using MasterOnline.Utils;
using MasterOnline.ViewModels;
using Newtonsoft.Json;

namespace MasterOnline.Controllers.Api
{
    public class MobileApiController : ApiController
    {
        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }
        private AccountUserViewModel _viewModel;

        public MobileApiController()
        {
            MoDbContext = new MoDbContext();
            _viewModel = new AccountUserViewModel();
        }

        protected override void Dispose(bool disposing)
        {
            MoDbContext.Dispose();
        }

        [System.Web.Http.Route("api/mobile/logging")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult LoggingIn([FromBody] Account account)
        {
            JsonApi result;
            string apiKey = "";

            var re = Request;
            var headers = re.Headers;

            if (headers.Contains("X-API-KEY"))
            {
                apiKey = headers.GetValues("X-API-KEY").First();
            }

            if (apiKey != "M@STERONLINE4P1K3Y")
            {
                result = new JsonApi()
                {
                    code = 400,
                    message = "Wrong API KEY!",
                    data = null
                };

                return Json(result);
            }

            var accFromDb = MoDbContext.Account.SingleOrDefault(a => a.Email == account.Email);

            if (accFromDb == null)
            {
                var userFromDb = MoDbContext.User.SingleOrDefault(a => a.Email == account.Email);

                if (userFromDb == null)
                {
                    result = new JsonApi()
                    {
                        code = 200,
                        message = "Email tidak ditemukan!",
                        data = null
                    };

                    return Json(result);
                }

                var pass = userFromDb?.Password;

                if (!account.Password.Equals(pass))
                {
                    result = new JsonApi()
                    {
                        code = 200,
                        message = "Password salah!",
                        data = null
                    };

                    return Json(result);
                }

                if (!userFromDb.Status)
                {
                    result = new JsonApi()
                    {
                        code = 200,
                        message = "Akun belum diaktifkan oleh admin!",
                        data = null
                    };

                    return Json(result);
                }

                _viewModel.User = userFromDb;
            }
            else
            {
                var pass = accFromDb.Password;
                var hashCode = accFromDb.VCode;
                var encodingPassString = Helper.EncodePassword(account.Password, hashCode);

                if (!encodingPassString.Equals(pass))
                {
                    result = new JsonApi()
                    {
                        code = 200,
                        message = "Password salah!",
                        data = null
                    };

                    return Json(result);
                }

                if (!accFromDb.Status)
                {
                    result = new JsonApi()
                    {
                        code = 200,
                        message = "Akun belum diaktifkan oleh admin!",
                        data = null
                    };

                    return Json(result);
                }

                _viewModel.Account = accFromDb;
            }

            if (_viewModel?.Account != null)
            {
                ErasoftDbContext = _viewModel.Account.UserId == "admin_manage" ? new ErasoftContext() : new ErasoftContext(_viewModel.Account.UserId);
            }
            else
            {
                var accFromUser = MoDbContext.Account.Single(a => a.AccountId == _viewModel.User.AccountId);
                ErasoftDbContext = new ErasoftContext(accFromUser.UserId);
            }

            result = new JsonApi()
            {
                code = 200,
                message = "Login Berhasil",
                data = new
                {
                    logged = true,
                    userId = _viewModel.Account != null ? _viewModel.Account.UserId : _viewModel.User.UserId.ToString()
                }
            };

            return Json(result);
        }
    }
}
