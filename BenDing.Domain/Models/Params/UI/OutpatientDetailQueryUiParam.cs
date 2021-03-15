using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenDing.Domain.Models.Dto.Base;

namespace BenDing.Domain.Models.Params.UI
{
  public  class OutpatientDetailQueryUiParam:PaginationDto
    { /// <summary>
        /// 用户id
        /// </summary>
        [Display(Name = "用户id")]
        [Required(ErrorMessage = "{0}不能为空!!!")]
        public string UserId { get; set; }
        /// <summary>
        /// 业务id
        /// </summary>

        public string BusinessId { get; set; }
        /// <summary>
        /// 医保交易码
        /// </summary>
        public string TransKey { get; set; }
    }
}
