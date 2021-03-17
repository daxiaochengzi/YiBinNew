﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenDing.Domain.Models.Dto.Web
{/// <summary>
/// 
/// </summary>
   public class QueryHospitalizationFeeDto
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 明细id
        /// </summary>
        public  string DetailId { get; set; }
        /// <summary>
        /// 上传标记（1上传,0未上传）
        /// </summary>
        public int UploadMark { get; set; }
        /// <summary>
        /// 医保编码
        /// </summary>
        public  string ProjectCode { get; set; }
        /// <summary>
        /// 医保名称
        /// </summary>
        public string ProjectName { get; set; }
        /// <summary>
        /// 医保类别
        /// </summary>
        public string ProjectCodeType { get; set; }
        /// <summary>
        /// 医保上传金额
        /// </summary>
        public decimal UploadAmount { get; set; }
        /// <summary>
        /// 项目等级
        /// </summary>
        public string ProjectLevel { get; set; }
        /// <summary>
        /// 自付比例
        /// </summary>
        public decimal SelfPayProportion { get; set; }
        /// <summary>
        /// 限制价格
        /// </summary>
        public decimal BlockPrice { get; set; }
        /// <summary>
        /// 限制使用说明
        /// </summary>
        public string BlockRemark { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        public  string Specification { get; set; }
        
        /// <summary>
        /// His编码
        /// </summary>
        public  string DirectoryCode { get; set; }
        /// <summary>
        /// His名称
        /// </summary>
        public string  DirectoryName { get; set; }
        /// <summary>
        /// his类别
        /// </summary>
        public string DirectoryCategoryCode { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public decimal UnitPrice { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// 费用日期
        /// </summary>
        public  DateTime BillTime { get; set; }
        /// <summary>
        /// 上传操作员
        /// </summary>
        public  string UploadUserName { get; set; }
        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime? UploadTime { get; set; }
        /// <summary>
        /// 医保批次号
        /// </summary>
        public string BatchNumber { get; set; }
        /// <summary>
        /// 处方号
        /// </summary>
        public  string RecipeCode { get; set; }
        /// <summary>
        /// 科室
        /// </summary>
        public  string BillDepartment { get; set; }
        /// <summary>
        /// 执行医生
        /// </summary>
        public string OperateDoctorName { get; set; }
        /// <summary>
        /// 调整差值
        /// </summary>
        public decimal AdjustmentDifferenceValue { get; set; }
        /// <summary>
        ///组织机构
        /// </summary>
        public  string OrganizationCode { get; set; }
        /// <summary>
        /// 医保限制用药标志
        /// </summary>

        public string RestrictionSign { get; set; }
        /// <summary>
        /// 限制审核标志
        /// </summary>
        public int  ApprovalMark { get; set; }
        /// <summary>
        /// 限制审核人员
        /// </summary>
        public string ApprovalUserName { get; set; }
        /// <summary>
        /// 基层生产厂家
        /// </summary>
        public string ManufacturerName { get; set; }
        /// <summary>
        /// 医保生产厂家
        /// </summary>
        public string Manufacturer { get; set; }
        /// <summary>
        ///   不传医保
        /// </summary>
        public  int? NotUploadMark  { get; set; }

}
}
