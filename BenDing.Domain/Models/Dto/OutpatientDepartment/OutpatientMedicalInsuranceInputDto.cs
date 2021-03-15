using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenDing.Domain.Models.Dto.Web;

namespace BenDing.Domain.Models.Dto.OutpatientDepartment
{
   public class OutpatientMedicalInsuranceInputDto
    {/// <summary>
     ///  基层保留四位小数
     /// </summary>
        public decimal OldTotalCost { get; set; }
        /// <summary>
        /// 基层费用合计
        /// </summary>
        public decimal NewTotalCost{ get; set; }
        /// <summary>
        /// 病人基础信息
        /// </summary>
        public BaseOutpatientInfoDto BaseOutpatient { get; set; }
        public List<BaseOutpatientDetailDto> DetailList { get; set; }
    }
}
