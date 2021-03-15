using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenDing.Domain.Models.Dto.Web;

namespace BenDing.Domain.Models.Dto.OutpatientDepartment
{
  public  class OutpatientDetailQueryDto
    {  /// <summary>
    /// 分页
    /// </summary>
        public List<BaseOutpatientDetailDto> PageData { get; set; }

        /// <summary>
        /// 新的费用合计
        /// </summary>
        public decimal NewTotalCost { get; set; }

        /// <summary>
        /// 医保合计费用
        /// </summary>
        public decimal MedicalInsuranceTotalCost { get; set; }
        /// <summary>
        /// 门诊号
        /// </summary>

        public string OutpatientNumber { get; set; }
    }
}
