using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace VacationHome.ClaimService.Application.DTOs.Admin
{
    public sealed class ClaimRowVersionRequest
    {
        public string RowVersion { get; init; } =
            string.Empty;
    }
}
