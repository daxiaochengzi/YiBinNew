using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenDing.Domain.Models.Params.OutpatientDepartment
{
    public class QueryOutpatientDetailListParam
    {
        /// <summary>
        /// 病人id
        /// </summary>
        public string PatientId { get; set; }
        /// <summary>
        /// 门诊号
        /// </summary>
        public string OutpatientNo { get; set; }
      /// <summary>
      /// 是否调整差值
      /// </summary>
        public int? IsAdjustmentDifferenceValue { get; set; }
    }
}
