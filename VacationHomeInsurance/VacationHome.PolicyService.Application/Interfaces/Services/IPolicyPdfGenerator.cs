using VacationHome.PolicyService.Application.Configuration;
using VacationHome.PolicyService.Domain.Entities;


namespace VacationHome.PolicyService.Application.Interfaces.Services
{
	public interface IPolicyPdfGenerator
	{
		byte[] Generate(
			Policy policy,
			int version,
			DateTime generatedAtUtc,
			PolicyDocumentOptions options);
	}
}
