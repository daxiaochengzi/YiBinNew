﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFine.Domain.BenDing.Infrastructure;

namespace NFine.Domain._03_Entity.BenDingManage
{
   public class InpatientEntity: IBenDingEntity<InpatientEntity>, IBenDingCreationAudited,IBenDingDeleteAudited, IBenDingModificationAudited
    {
        /// <summary>
        /// 初始化住院病人
        /// </summary>
        /// <param name="id">住院病人标识</param>
        public Guid Id { get; set; }

        /// <summary>
        /// 住院Id
        /// </summary>
        [DisplayName("住院Id")]
        [StringLength(32)]
        public string HospitalizationId { get; set; }
        /// <summary>
        /// 住院号
        /// </summary>
        [DisplayName("住院号")]
        [StringLength(50)]
        public string HospitalizationNo { get; set; }
        /// <summary>
        /// 入院日期
        /// </summary>
        [DisplayName("入院日期")]
        public DateTime? AdmissionDate { get; set; }
        /// <summary>
        /// 出院日期
        /// </summary>
        [DisplayName("出院日期")]
        public DateTime? LeaveHospitalDate { get; set; }
        /// <summary>
        /// 业务ID/住院ID
        /// </summary>
        [DisplayName("业务ID/住院ID")]
        [StringLength(50)]
        public string BusinessId { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        [DisplayName("姓名")]
        [StringLength(20)]
        public string PatientName { get; set; }
        /// <summary>
        /// 身份证号码
        /// </summary>
        [DisplayName("身份证号码")]
        [StringLength(50)]
        public string IdCardNo { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        [DisplayName("性别")]
        [StringLength(2)]
        public string PatientSex { get; set; }
        /// <summary>
        /// 诊断Json
        /// </summary>
        [DisplayName("诊断Json")]
        public string DiagnosisJson { get; set; }
        /// <summary>
        /// 主要诊断
        /// </summary>
        [DisplayName("主要诊断")]
        [StringLength(1000)]
        public string AdmissionMainDiagnosis { get; set; }
        /// <summary>
        /// 入院诊断
        /// </summary>
        [DisplayName("入院诊断")]
        [StringLength(100)]
        public string AdmissionMainDiagnosisIcd10 { get; set; }
        /// <summary>
        /// 单据号
        /// </summary>
        [DisplayName("单据号")]
        [StringLength(32)]
        public string DocumentNo { get; set; }
        /// <summary>
        /// 联系人姓名
        /// </summary>
        [DisplayName("联系人姓名")]
        [StringLength(20)]
        public string ContactName { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        [DisplayName("联系电话")]
        [StringLength(20)]
        public string ContactPhone { get; set; }
        /// <summary>
        /// 家庭地址
        /// </summary>
        [DisplayName("家庭地址")]
        [StringLength(100)]
        public string FamilyAddress { get; set; }
        /// <summary>
        /// 入院科室
        /// </summary>
        [DisplayName("入院科室")]
        [StringLength(100)]
        public string InDepartmentName { get; set; }
        /// <summary>
        /// 入院科室编码
        /// </summary>
        [DisplayName("入院科室编码")]
        [StringLength(50)]
        public string InDepartmentId { get; set; }
        /// <summary>
        /// 入院诊断医生
        /// </summary>
        [DisplayName("入院诊断医生")]
        [StringLength(20)]
        public string AdmissionDiagnosticDoctor { get; set; }
        /// <summary>
        /// 入院床位
        /// </summary>
        [DisplayName("入院床位")]
        [StringLength(50)]
        public string AdmissionBed { get; set; }
        /// <summary>
        /// 入院经办人
        /// </summary>
        [DisplayName("入院经办人")]
        [StringLength(20)]
        public string AdmissionOperator { get; set; }
        /// <summary>
        /// 入院经办时间
        /// </summary>
        [DisplayName("入院经办时间")]
        public DateTime? AdmissionOperateTime { get; set; }
        /// <summary>
        /// 住院总费用
        /// </summary>
        [DisplayName("住院总费用")]
        public decimal? HospitalizationTotalCost { get; set; }
        /// <summary>
        /// 总费用
        /// </summary>
        [DisplayName("总费用")]
        public decimal? TotalFee { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [DisplayName("备注")]
        [StringLength(100)]
        public string Remark { get; set; }
        /// <summary>
        /// 入院诊断医生编码
        /// </summary>
        [DisplayName("入院诊断医生编码")]
        [StringLength(50)]
        public string AdmissionDiagnosticDoctorId { get; set; }
        /// <summary>
        /// 组织机构编码
        /// </summary>
        [DisplayName("组织机构编码")]
        [StringLength(50)]
        public string OrganizationCode { get; set; }
        /// <summary>
        /// 组织机构名称
        /// </summary>
        [DisplayName("组织机构名称")]
        [StringLength(200)]
        public string OrganizationName { get; set; }
        /// <summary>
        /// 出院诊断json
        /// </summary>
        [DisplayName("出院诊断json")]
        public string LeaveHospitalDiagnosisJson { get; set; }

        /// <summary>
        /// 出院科室编码
        /// </summary>
        [DisplayName("出院科室编码")]
        [StringLength(100)]
        public string LeaveHospitalDepartmentId { get; set; }
        /// <summary>
        /// 出院科室名称
        /// </summary>
        [DisplayName("出院科室名称")]
        [StringLength(100)]
        public string LeaveHospitalDepartmentName { get; set; }

        /// <summary>
        /// 出院床位
        /// </summary>
        [DisplayName("出院床位")]
        [StringLength(100)]
        public string LeaveHospitalBedNumber { get; set; }
        /// <summary>
        /// 出院诊断医生
        /// </summary>
        [DisplayName("出院诊断医生")]
        [StringLength(100)]
     
        public string LeaveHospitalDiagnosticDoctor { get; set; }
        /// <summary>
        /// 出院经办人
        /// </summary>
        [DisplayName("出院经办人")]
        [StringLength(100)]
        public string LeaveHospitalOperator { get; set; }
        /// <summary>
        /// 是否取消基层入院登记
        /// </summary>
        [DisplayName("是否取消基层入院登记")]
        public int? IsCanCelHospitalized { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [DisplayName("创建时间")]
        public DateTime? CreateTime { get; set; }
        /// <summary>
        /// 删除标记[0:默认,1:删除]
        /// </summary>
        [DisplayName("删除标记[0:默认,1:删除]")]
        public bool IsDelete { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        [DisplayName("更新时间")]
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 删除时间
        /// </summary>
        [DisplayName("删除时间")]
        public DateTime? DeleteTime { get; set; }
        /// <summary>
        /// 操作员ID-[创建]
        /// </summary>
        [DisplayName("操作员ID-[创建]")]
        [StringLength(100)]
        public string CreateUserId { get; set; }
        /// <summary>
        /// 操作员ID-[删除]
        /// </summary>
        [DisplayName("操作员ID-[删除]")]
        [StringLength(100)]
        public string DeleteUserId { get; set; }
        /// <summary>
        /// 更新人员
        /// </summary>
        [DisplayName("更新人员")]
        [StringLength(100)]
        public string UpdateUserId { get; set; }
    }
}
