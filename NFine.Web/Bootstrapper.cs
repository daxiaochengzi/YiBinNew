using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dependencies;
using BenDing.Repository.Interfaces.Web;
using BenDing.Repository.Providers.Web;
using BenDing.Service.Interfaces;
using BenDing.Service.Providers;
using NFine.Web.Model;
using StructureMap;
using Unity;

namespace NFine.Web
{/// <summary>
/// 
/// </summary>
    public class Bootstrapper
    {/// <summary>
    /// 
    /// </summary>
        public static void Initialise()
        {
            var container = BuildUnityContainer();
            // DependencyResolver.SetResolver(new UnityDependencyResolver(container));//MVCע��
            GlobalConfiguration.Configuration.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);

        }/// <summary>
        /// 
        /// </summary>
        public class UnityIOC
        {
            /// <summary>
            /// IOC ����
            /// </summary>
            public static IUnityContainer container = null;

            /// <summary>
            /// ��ȡ �ӿ� ��ʵ��������
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public static T Resolve<T>()
            {
                try
                {
                    if (container == null)
                    {
                        Initialise();
                    }
                }
                catch (System.Exception ex)
                {
                    throw new System.Exception("IOCʵ��������!" + ex.Message);
                }

                return container.Resolve<T>();
            }

            /// <summary>
            /// ��ʼ��
            /// </summary>
            public static void Initialise()
            {
                ////��������
                //container = new UnityContainer();
                ////ע��ӳ��
                //container.RegisterType<IUserService, UserService>();
                container = BuildUnityContainer();
            }
        }
        private static IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();
            #region Service
            container.RegisterType<IUserService, UserService>();
            container.RegisterType<IWebServiceBasicService, WebServiceBasicService>();
            container.RegisterType<IResidentMedicalInsuranceService, ResidentMedicalInsuranceService>();
            container.RegisterType<IOutpatientDepartmentService, OutpatientDepartmentService>();
            container.RegisterType<IWorkerMedicalInsuranceService, WorkerMedicalInsuranceService>();
            container.RegisterType<IOutpatientDepartmentNewService, OutpatientDepartmentNewService>();
            container.RegisterType<IResidentMedicalInsuranceNewService, ResidentMedicalInsuranceNewService>();
            container.RegisterType<IWorkerMedicalInsuranceNewService, WorkerMedicalInsuranceNewService>();
            
            #endregion
            #region Repository
            container.RegisterType<IResidentMedicalInsuranceRepository, ResidentMedicalInsuranceRepository>();
            container.RegisterType<IMedicalInsuranceSqlRepository, MedicalInsuranceSqlRepository>();
            container.RegisterType<IWebBasicRepository, WebBasicRepository>();
            container.RegisterType<IHisSqlRepository, HisSqlRepository>();
            container.RegisterType<ISystemManageRepository, SystemManageRepository>();
            container.RegisterType<IOutpatientDepartmentRepository, OutpatientDepartmentRepository>();
            container.RegisterType<IWorkerMedicalInsuranceRepository, WorkerMedicalInsuranceRepository>();
            container.RegisterType<ISqlSugarRepository, SqlSugarRepository>();
            #endregion
            return container;
        }
        
    }

}