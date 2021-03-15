using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenDing.Domain.Models.Params.Base;

namespace BenDing.Domain.Models.Params.UI
{
   public class OutpatientAdjustmentDifferenceValueUiParam: UiBaseDataParam
    {
        /// <summary>
        /// 明细id
        /// </summary>
        public string DetailId { get; set; }
        /// <summary>
        /// 门诊号
        /// </summary>
        public  string OutpatientNo { get; set; }
        /// <summary>
        /// 合计金额
        /// </summary>
        public  decimal Amount { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Quantity { get; set; }
    }
}
