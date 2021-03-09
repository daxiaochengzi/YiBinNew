/*******************************************************************************
 * Copyright © 2016 NFine.Framework 版权所有
 * Author: NFine
 * Description: NFine快速开发平台
 * Website：http://www.nfine.cn
*********************************************************************************/
using NFine.Application.SystemManage;
using NFine.Code;
using NFine.Domain.Entity.SystemManage;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using NFine.Web.Model;

namespace NFine.Web.Controllers
{
    [HandlerLogin]
    public class HomeController : Controller
    {
        private UserApp userApp = new UserApp();
        private OrganizeApp organizeApp =new OrganizeApp();

        public HomeController()
        {
            
        }
        [HttpGet]
        public ActionResult Index()
        {
            var loginInfo = OperatorProvider.Provider.GetCurrent();
            if (loginInfo != null)
            {
               var organizeData= organizeApp.GetForm(loginInfo.DepartmentId);
                ViewBag.LoginOrganizationName = organizeData.F_FullName;
            }

            return View();
        }
        [HttpGet]
        public ActionResult Default()
        {
            return View();
        }
        [HttpGet]
        public ActionResult About()
        {
            return View();
        }
    }
}
