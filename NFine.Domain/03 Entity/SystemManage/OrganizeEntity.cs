﻿/*******************************************************************************
 * Copyright © 2016 NFine.Framework 版权所有
 * Author: NFine
 * Description: NFine快速开发平台
 * Website：http://www.nfine.cn
*********************************************************************************/
using System;

namespace NFine.Domain.Entity.SystemManage
{
    public class OrganizeEntity : IEntity<OrganizeEntity>, ICreationAudited, IDeleteAudited, IModificationAudited
    {
        public string F_Id { get; set; }
        public string F_ParentId { get; set; }
        public int? F_Layers { get; set; }
        public string F_EnCode { get; set; }
        public string F_FullName { get; set; }
        public string F_ShortName { get; set; }
        public string F_CategoryId { get; set; }
        public string F_ManagerId { get; set; }
        public string F_TelePhone { get; set; }
        public string F_MobilePhone { get; set; }
        public string F_WeChat { get; set; }
        public string F_Fax { get; set; }
        public string F_Email { get; set; }
        public string F_AreaId { get; set; }
        public string F_Address { get; set; }
        public bool? F_AllowEdit { get; set; }
        public bool? F_AllowDelete { get; set; }
        public int? F_SortCode { get; set; }
        public bool? F_DeleteMark { get; set; }
        public bool? F_EnabledMark { get; set; }
        public string F_Description { get; set; }
        public DateTime? F_CreatorTime { get; set; }
        public string F_CreatorUserId { get; set; }
        public DateTime? F_LastModifyTime { get; set; }
        public string F_LastModifyUserId { get; set; }
        public DateTime? F_DeleteTime { get; set; }
        public string F_DeleteUserId { get; set; }
        /// <summary>
        /// 医院id
        /// </summary>
        public string F_HospitalId { get; set; }
        /// <summary>
        ///  医院等级
        /// </summary>
        public int F_OrganizationGrade { get; set; }
        /// <summary>
        /// 医院行政区域
        /// </summary>
        public string F_AdministrativeArea { get; set; }
        /// <summary>
        /// 医保账户
        /// </summary>
        public string F_MedicalInsuranceAccount { get; set; }
        /// <summary>
        /// 医保密码
        /// </summary>
        public string F_MedicalInsurancePwd { get; set; }
        /// <summary>
        /// 医保密码
        /// </summary>
        public string F_MedicalInsuranceHandleNo { get; set; }
        /// <summary>
        /// 有效时间
        /// </summary>
        public DateTime? F_EffectiveTime { get; set; }
        /// <summary>
        /// 门诊
        /// </summary>
        public bool? F_Outpatient { get; set; }
        /// <summary>
        /// 住院
        /// </summary>
        public bool? F_Hospital { get; set; }
        /// <summary>
        /// 生育
        /// </summary>
        public bool? F_BirthHospital { get; set; }
        /// <summary>
        /// 异地
        /// </summary>
        public bool? F_AnotherPlace { get; set; }
    }
}
