using System.Text.Json;
using VacationHome.Customer.Application.Common;
using VacationHome.Customer.Application.DTOs.Customers;
using VacationHome.Customer.Application.DTOs.Properties;
using VacationHome.Customer.Application.Exceptions;
using VacationHome.Customer.Application.Interfaces.Repositories;
using VacationHome.Customer.Application.Interfaces.Services;
using VacationHome.Customer.Application.Mappers;
using VacationHome.Customer.Domain.Entities;
using VacationHome.Customer.Domain.Enums;


namespace VacationHome.Customer.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository
            _customerRepository;


        private readonly ICustomerProfileRepository
            _customerProfileRepository;


        private readonly IVacationPropertyRepository
            _vacationPropertyRepository;


        private readonly IAuditLogRepository
            _auditLogRepository;


        private readonly IUnitOfWork
            _unitOfWork;


        private readonly ICurrentUserService
            _currentUserService;


        public CustomerService(
            ICustomerRepository customerRepository,
            ICustomerProfileRepository customerProfileRepository,
            IVacationPropertyRepository vacationPropertyRepository,
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _customerRepository =
                customerRepository;


            _customerProfileRepository =
                customerProfileRepository;


            _vacationPropertyRepository =
                vacationPropertyRepository;


            _auditLogRepository =
                auditLogRepository;


            _unitOfWork =
                unitOfWork;


            _currentUserService =
                currentUserService;
        }


        public async Task<CustomerProfileResponse>
            GetProfileAsync(
                CancellationToken cancellationToken)
        {
            var customer =
                await GetActiveCustomerAsync(
                    cancellationToken);


            var profile =
                await _customerProfileRepository
                    .GetByCustomerIdAsync(
                        customer.Id,
                        cancellationToken);


            return CustomerMapper.ToResponse(
                customer,
                profile);
        }


        public async Task UpdateProfileAsync(
            UpdateCustomerProfileRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);


            var customer =
                await GetActiveCustomerAsync(
                    cancellationToken);


            var profile =
                await _customerProfileRepository
                    .GetByCustomerIdAsync(
                        customer.Id,
                        cancellationToken);


            var utcNow =
                DateTime.UtcNow;


            if (profile is null)
            {
                profile = new CustomerProfile
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    CreatedAtUtc = utcNow
                };


                await _customerProfileRepository.AddAsync(
                    profile,
                    cancellationToken);
            }


            /*
             * PATCH-style behavior:
             *
             * null means the field was not supplied and therefore
             * remains unchanged.
             *
             * An empty or whitespace string clears an optional
             * string field because NormalizeOptional returns null.
             *
             * DateOfBirth cannot currently be explicitly cleared.
             */


            if (request.DateOfBirth.HasValue)
            {
                profile.DateOfBirth =
                    request.DateOfBirth.Value.Date;
            }


            if (request.AddressLine1 is not null)
            {
                profile.AddressLine1 =
                    NormalizeOptional(
                        request.AddressLine1);
            }


            if (request.AddressLine2 is not null)
            {
                profile.AddressLine2 =
                    NormalizeOptional(
                        request.AddressLine2);
            }


            if (request.City is not null)
            {
                profile.City =
                    NormalizeOptional(
                        request.City);
            }


            if (request.State is not null)
            {
                profile.State =
                    NormalizeOptional(
                        request.State);
            }


            if (request.PostalCode is not null)
            {
                profile.PostalCode =
                    NormalizeOptional(
                        request.PostalCode);
            }


            if (request.Country is not null)
            {
                profile.Country =
                    NormalizeOptional(
                        request.Country);
            }


            profile.UpdatedAtUtc =
                utcNow;


            await AddAuditAsync(
                entityName: "CustomerProfile",
                entityId: profile.Id,
                action: "ProfileUpdated",
                oldValues: null,
                newValues: null,
                performedAtUtc: utcNow,
                cancellationToken:
                    cancellationToken);


            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }


        public async Task<Guid> CreatePropertyAsync(
            CreateVacationPropertyRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);


            var customer =
                await GetActiveCustomerAsync(
                    cancellationToken);


            var normalizedPropertyName =
                NormalizeRequired(
                    request.PropertyName);


            var hasDuplicateName =
                await _vacationPropertyRepository
                    .HasDuplicatePropertyNameAsync(
                        customerId: customer.Id,
                        propertyName: normalizedPropertyName,
                        ignorePropertyId: null,
                        cancellationToken: cancellationToken);


            if (hasDuplicateName)
            {
                throw new ConflictException(
                    ErrorCodes.PropertyNameAlreadyExists,
                    "A property with this name already exists. Please choose a different property name.");
            }


            var utcNow =
                DateTime.UtcNow;


            var property =
                new VacationProperty
                {
                    Id = Guid.NewGuid(),


                    CustomerId =
                        customer.Id,


                    PropertyName =
                        normalizedPropertyName,


                    PropertyType =
                        request.PropertyType,


                    AddressLine1 =
                        NormalizeRequired(
                            request.AddressLine1),


                    AddressLine2 =
                        NormalizeOptional(
                            request.AddressLine2),


                    City =
                        NormalizeRequired(
                            request.City),


                    State =
                        NormalizeRequired(
                            request.State),


                    PostalCode =
                        NormalizeRequired(
                            request.PostalCode),


                    Country =
                        NormalizeRequired(
                            request.Country),


                    PropertyValue =
                        request.PropertyValue,


                    ConstructionYear =
                        request.ConstructionYear,


                    OccupancyFrequency =
                        request.OccupancyFrequency,


                    HasSecuritySystem =
                        request.HasSecuritySystem,


                    PreviousClaimsCount =
                        request.PreviousClaimsCount,


                    DisasterExposure =
                        request.DisasterExposure,


                    IsArchived = false,


                    ArchivedAtUtc = null,


                    CreatedAtUtc =
                        utcNow
                };


            await _vacationPropertyRepository.AddAsync(
                property,
                cancellationToken);


            await AddAuditAsync(
                entityName: "VacationProperty",
                entityId: property.Id,
                action: "PropertyCreated",
                oldValues: null,
                newValues: null,
                performedAtUtc: utcNow,
                cancellationToken:
                    cancellationToken);


            await _unitOfWork.SaveChangesAsync(
                cancellationToken);


            return property.Id;
        }


        public async Task<
            PagedResult<VacationPropertyResponse>>
            GetPropertiesAsync(
                PropertyQueryParameters query,
                CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);


            var customer =
                await GetActiveCustomerAsync(
                    cancellationToken);


            var sortDescending =
                string.Equals(
                    query.SortDirection,
                    "desc",
                    StringComparison.OrdinalIgnoreCase);


            var result =
                await _vacationPropertyRepository
                    .GetPagedByCustomerIdAsync(
                        customerId: customer.Id,
                        pageNumber: query.PageNumber,
                        pageSize: query.PageSize,
                        includeArchived:
                            query.IncludeArchived,
                        sortBy: query.SortBy,
                        sortDescending:
                            sortDescending,
                        cancellationToken:
                            cancellationToken);


            return new PagedResult<
                VacationPropertyResponse>
            {
                Items = result.Items
                    .Select(
                        VacationPropertyMapper
                            .ToResponse)
                    .ToList(),


                PageNumber =
                    query.PageNumber,


                PageSize =
                    query.PageSize,


                TotalCount =
                    result.TotalCount
            };
        }


        public async Task<VacationPropertyResponse>
            GetPropertyAsync(
                Guid propertyId,
                CancellationToken cancellationToken)
        {
            var customer =
                await GetActiveCustomerAsync(
                    cancellationToken);


            var property =
                await _vacationPropertyRepository
                    .GetOwnedPropertyReadOnlyAsync(
                        customerId: customer.Id,
                        propertyId: propertyId,
                        includeArchived: true,
                        cancellationToken:
                            cancellationToken);


            if (property is null)
            {
                throw new NotFoundException(
                    ErrorCodes.PropertyNotFound,
                    "Property not found.");
            }


            return VacationPropertyMapper.ToResponse(
                property);
        }


        public async Task UpdatePropertyAsync(
            Guid propertyId,
            UpdateVacationPropertyRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);


            var customer =
                await GetActiveCustomerAsync(
                    cancellationToken);


            var property =
                await _vacationPropertyRepository
                    .GetOwnedPropertyForUpdateAsync(
                        customerId: customer.Id,
                        propertyId: propertyId,
                        includeArchived: true,
                        cancellationToken:
                            cancellationToken);


            if (property is null)
            {
                throw new NotFoundException(
                    ErrorCodes.PropertyNotFound,
                    "Property not found.");
            }


            if (property.IsArchived)
            {
                throw new ConflictException(
                    ErrorCodes.PropertyArchived,
                    "An archived property cannot be updated.");
            }


            var suppliedRowVersion =
                DecodeRowVersion(
                    request.RowVersion);


            var normalizedPropertyName =
                NormalizeRequired(
                    request.PropertyName);


            var hasDuplicateName =
                await _vacationPropertyRepository
                    .HasDuplicatePropertyNameAsync(
                        customerId: customer.Id,
                        propertyName: normalizedPropertyName,
                        ignorePropertyId: propertyId,
                        cancellationToken: cancellationToken);


            if (hasDuplicateName)
            {
                throw new ConflictException(
                    ErrorCodes.PropertyNameAlreadyExists,
                    "A property with this name already exists. Please choose a different property name.");
            }


            _vacationPropertyRepository
                .SetOriginalRowVersion(
                    property,
                    suppliedRowVersion);


            var oldRiskValues =
                SerializeRiskRelevantValues(
                    property);


            var utcNow =
                DateTime.UtcNow;


            property.PropertyName =
                normalizedPropertyName;


            property.PropertyType =
                request.PropertyType;


            property.AddressLine1 =
                NormalizeRequired(
                    request.AddressLine1);


            property.AddressLine2 =
                NormalizeOptional(
                    request.AddressLine2);


            property.City =
                NormalizeRequired(
                    request.City);


            property.State =
                NormalizeRequired(
                    request.State);


            property.PostalCode =
                NormalizeRequired(
                    request.PostalCode);


            property.Country =
                NormalizeRequired(
                    request.Country);


            property.PropertyValue =
                request.PropertyValue;


            property.ConstructionYear =
                request.ConstructionYear;


            property.OccupancyFrequency =
                request.OccupancyFrequency;


            property.HasSecuritySystem =
                request.HasSecuritySystem;


            property.PreviousClaimsCount =
                request.PreviousClaimsCount;


            property.DisasterExposure =
                request.DisasterExposure;


            property.UpdatedAtUtc =
                utcNow;


            var newRiskValues =
                SerializeRiskRelevantValues(
                    property);


            await AddAuditAsync(
                entityName: "VacationProperty",
                entityId: property.Id,
                action: "PropertyUpdated",
                oldValues: oldRiskValues,
                newValues: newRiskValues,
                performedAtUtc: utcNow,
                cancellationToken:
                    cancellationToken);


            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }


        public async Task ArchivePropertyAsync(
            Guid propertyId,
            ArchiveVacationPropertyRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);


            var customer =
                await GetActiveCustomerAsync(
                    cancellationToken);


            var property =
                await _vacationPropertyRepository
                    .GetOwnedPropertyForUpdateAsync(
                        customerId: customer.Id,
                        propertyId: propertyId,
                        includeArchived: true,
                        cancellationToken:
                            cancellationToken);


            if (property is null)
            {
                throw new NotFoundException(
                    ErrorCodes.PropertyNotFound,
                    "Property not found.");
            }


            if (property.IsArchived)
            {
                throw new ConflictException(
                    ErrorCodes.PropertyAlreadyArchived,
                    "The property is already archived.");
            }


            var suppliedRowVersion =
                DecodeRowVersion(
                    request.RowVersion);


            _vacationPropertyRepository
                .SetOriginalRowVersion(
                    property,
                    suppliedRowVersion);


            var utcNow =
                DateTime.UtcNow;


            var oldValues =
                JsonSerializer.Serialize(
                    new
                    {
                        IsArchived = false,
                        ArchivedAtUtc =
                            (DateTime?)null
                    });


            property.IsArchived =
                true;


            property.ArchivedAtUtc =
                utcNow;


            property.UpdatedAtUtc =
                utcNow;


            var newValues =
                JsonSerializer.Serialize(
                    new
                    {
                        property.IsArchived,
                        property.ArchivedAtUtc
                    });


            await AddAuditAsync(
                entityName: "VacationProperty",
                entityId: property.Id,
                action: "PropertyArchived",
                oldValues: oldValues,
                newValues: newValues,
                performedAtUtc: utcNow,
                cancellationToken:
                    cancellationToken);


            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }


        private async Task<Customers>
            GetActiveCustomerAsync(
                CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsAuthenticated)
            {
                throw new UnauthorizedException(
                    "Authentication is required.");
            }


            var authUserId =
                _currentUserService.UserId?.Trim();


            if (string.IsNullOrWhiteSpace(
                    authUserId))
            {
                throw new UnauthorizedException(
                    "The authenticated user identifier is missing.");
            }


            var customer =
                await _customerRepository
                    .GetByAuthUserIdAsync(
                        authUserId,
                        cancellationToken);


            if (customer is null)
            {
                throw new NotFoundException(
                    ErrorCodes.CustomerNotFound,
                    "Customer not found.");
            }


            if (customer.Status !=
                CustomerStatus.Active)
            {
                throw new ForbiddenException(
                    ErrorCodes.CustomerInactive,
                    "The customer account is inactive.");
            }


            return customer;
        }


        private async Task AddAuditAsync(
            string entityName,
            Guid entityId,
            string action,
            string? oldValues,
            string? newValues,
            DateTime performedAtUtc,
            CancellationToken cancellationToken)
        {
            var performedBy =
                string.IsNullOrWhiteSpace(
                    _currentUserService.UserId)
                    ? "Unknown"
                    : _currentUserService.UserId.Trim();


            var performedByRole =
                string.IsNullOrWhiteSpace(
                    _currentUserService.Role)
                    ? "Customer"
                    : _currentUserService.Role.Trim();


            var correlationId =
                string.IsNullOrWhiteSpace(
                    _currentUserService.CorrelationId)
                    ? null
                    : _currentUserService
                        .CorrelationId
                        .Trim();


            await _auditLogRepository.AddAsync(
                new AuditLog
                {
                    Id = Guid.NewGuid(),


                    EntityName =
                        entityName,


                    EntityId =
                        entityId.ToString(),


                    Action =
                        action,


                    OldValues =
                        oldValues,


                    NewValues =
                        newValues,


                    PerformedBy =
                        performedBy,


                    PerformedByRole =
                        performedByRole,


                    CorrelationId =
                        correlationId,


                    PerformedAtUtc =
                        performedAtUtc
                },
                cancellationToken);
        }


        private static byte[] DecodeRowVersion(
            string rowVersion)
        {
            if (string.IsNullOrWhiteSpace(
                    rowVersion))
            {
                throw new BadRequestException(
                    ErrorCodes.InvalidRowVersion,
                    "Row version is required.");
            }


            try
            {
                var decodedRowVersion =
                    Convert.FromBase64String(
                        rowVersion.Trim());


                if (decodedRowVersion.Length == 0)
                {
                    throw new BadRequestException(
                        ErrorCodes.InvalidRowVersion,
                        "Row version is invalid.");
                }


                return decodedRowVersion;
            }
            catch (FormatException)
            {
                throw new BadRequestException(
                    ErrorCodes.InvalidRowVersion,
                    "Row version is invalid.");
            }
        }


        private static string NormalizeRequired(
            string value)
        {
            /*
             * FluentValidation should already reject null,
             * empty, and whitespace-only required values.
             * This guard protects non-controller callers.
             */
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new BadRequestException(
                    "INVALID_REQUIRED_VALUE",
                    "A required value is missing.");
            }


            return value.Trim();
        }


        private static string? NormalizeOptional(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }


        private static string
            SerializeRiskRelevantValues(
                VacationProperty property)
        {
            return JsonSerializer.Serialize(
                new
                {
                    property.PropertyValue,
                    property.ConstructionYear,
                    property.OccupancyFrequency,
                    property.HasSecuritySystem,
                    property.PreviousClaimsCount,
                    property.DisasterExposure
                });
        }
    }
}
