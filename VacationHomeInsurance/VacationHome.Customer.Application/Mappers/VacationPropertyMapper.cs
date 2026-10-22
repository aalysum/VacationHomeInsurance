using VacationHome.Customer.Application.DTOs.Internal;
using VacationHome.Customer.Application.DTOs.Properties;
using VacationHome.Customer.Domain.Entities;


namespace VacationHome.Customer.Application.Mappers
{
    public static class VacationPropertyMapper
    {
        public static VacationPropertyResponse ToResponse(
            VacationProperty property)
        {
            return new VacationPropertyResponse
            {
                PropertyId = property.Id,
                PropertyName = property.PropertyName,
                PropertyType = property.PropertyType,
                AddressLine1 = property.AddressLine1,
                AddressLine2 = property.AddressLine2,
                City = property.City,
                State = property.State,
                PostalCode = property.PostalCode,
                Country = property.Country,
                PropertyValue = property.PropertyValue,
                ConstructionYear = property.ConstructionYear,
                PropertyAge = CalculatePropertyAge(
                    property.ConstructionYear),
                OccupancyFrequency =
                    property.OccupancyFrequency,
                HasSecuritySystem =
                    property.HasSecuritySystem,
                PreviousClaimsCount =
                    property.PreviousClaimsCount,
                DisasterExposure =
                    property.DisasterExposure,
                IsArchived = property.IsArchived,
                ArchivedAtUtc = property.ArchivedAtUtc,
                CreatedAtUtc = property.CreatedAtUtc,
                UpdatedAtUtc = property.UpdatedAtUtc,
                RowVersion = Convert.ToBase64String(
                    property.RowVersion)
            };
        }


        public static InternalPropertySnapshotResponse
            ToInternalSnapshot(
                VacationProperty property,
                Customers customer)
        {
            return new InternalPropertySnapshotResponse
            {
                PropertyId = property.Id,
                CustomerId = customer.Id,
                AuthUserId = customer.AuthUserId,
                PropertyName = property.PropertyName,
                PropertyType = property.PropertyType,
                AddressLine1 = property.AddressLine1,
                AddressLine2 = property.AddressLine2,
                City = property.City,
                State = property.State,
                PostalCode = property.PostalCode,
                Country = property.Country,
                PropertyValue = property.PropertyValue,
                ConstructionYear = property.ConstructionYear,
                PropertyAge = CalculatePropertyAge(
                    property.ConstructionYear),
                OccupancyFrequency =
                    property.OccupancyFrequency,
                HasSecuritySystem =
                    property.HasSecuritySystem,
                PreviousClaimsCount =
                    property.PreviousClaimsCount,
                DisasterExposure =
                    property.DisasterExposure,
                IsArchived = property.IsArchived,
                SourceVersion = Convert.ToBase64String(
                    property.RowVersion),
                UpdatedAtUtc = property.UpdatedAtUtc
            };
        }


        private static int CalculatePropertyAge(
            int constructionYear)
        {
            return Math.Max(
                0,
                DateTime.UtcNow.Year - constructionYear);
        }
    }
}
