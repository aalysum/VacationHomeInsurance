using System.Text.Json;
using VacationHome.Customer.Application.Common;
using VacationHome.Customer.Application.DTOs.Customers;
using VacationHome.Customer.Application.Exceptions;
using VacationHome.Customer.Application.Interfaces.Repositories;
using VacationHome.Customer.Application.Interfaces.Services;
using VacationHome.Customer.Domain.Entities;
using VacationHome.Customer.Domain.Enums;


namespace VacationHome.Customer.Application.Services
{
    public class CustomerOnboardingService
        : ICustomerOnboardingService
    {
        private readonly ICustomerRepository
            _customerRepository;


        private readonly IAuditLogRepository
            _auditLogRepository;


        private readonly IUnitOfWork
            _unitOfWork;


        private readonly ICurrentUserService
            _currentUserService;


        private readonly ICustomerOnboardingPersistence
            _onboardingPersistence;
        public CustomerOnboardingService(
            ICustomerRepository customerRepository,
            ICustomerOnboardingPersistence onboardingPersistence,
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _customerRepository =
                customerRepository;


            _onboardingPersistence =
                onboardingPersistence;


            _auditLogRepository =
                auditLogRepository;


            _unitOfWork =
                unitOfWork;


            _currentUserService =
                currentUserService;
        }


        public async Task<CreateCustomerResponse>
            CreateCustomerAsync(
                CreateCustomerRequest request,
                CancellationToken cancellationToken)
        {
            var authUserId =
                request.AuthUserId.Trim();


            var firstName =
                request.FirstName.Trim();


            var lastName =
                request.LastName.Trim();


            var phoneNumber =
                request.PhoneNumber.Trim();


            var existingCustomer =
                await _customerRepository.GetByAuthUserIdAsync(
                    authUserId,
                    cancellationToken);


            if (existingCustomer is not null)
            {
                EnsureExistingCustomerMatches(
                    existingCustomer,
                    firstName,
                    lastName,
                    phoneNumber);


                return new CreateCustomerResponse
                {
                    CustomerId = existingCustomer.Id,
                    AlreadyExisted = true
                };
            }


            var customer = new Customers
            {
                Id = Guid.NewGuid(),
                AuthUserId = authUserId,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                Status = CustomerStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };


            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityName = "Customer",
                EntityId = customer.Id.ToString(),
                Action = "CustomerCreated",
                PerformedBy = GetInternalActor(),
                PerformedByRole = _currentUserService.Role,
                CorrelationId =
                    _currentUserService.CorrelationId,
                PerformedAtUtc = DateTime.UtcNow
            };


            var result =
                await _onboardingPersistence.TryCreateAsync(
                    customer,
                    auditLog,
                    cancellationToken);


            if (result ==
                CustomerOnboardingSaveResult.Created)
            {
                return new CreateCustomerResponse
                {
                    CustomerId = customer.Id,
                    AlreadyExisted = false
                };
            }


            existingCustomer =
                await _customerRepository.GetByAuthUserIdAsync(
                    authUserId,
                    cancellationToken);


            if (existingCustomer is null)
            {
                throw new ConflictException(
                    ErrorCodes.CustomerOnboardingConflict,
                    "Customer onboarding could not be completed.");
            }


            EnsureExistingCustomerMatches(
                existingCustomer,
                firstName,
                lastName,
                phoneNumber);


            return new CreateCustomerResponse
            {
                CustomerId = existingCustomer.Id,
                AlreadyExisted = true
            };
        }






        private string GetInternalActor()
        {
            return string.IsNullOrWhiteSpace(
                _currentUserService.UserId)
                ? "AuthService"
                : _currentUserService.UserId;
        }
        public async Task UpdateCustomerStatusAsync(
            string authUserId,
            UpdateCustomerStatusRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(authUserId))
            {
                throw new BadRequestException(
                    ErrorCodes.InvalidAuthUserId,
                    "Authentication user identifier is required.");
            }


            var customer =
                await _customerRepository.GetByAuthUserIdAsync(
                    authUserId.Trim(),
                    cancellationToken);


            if (customer is null)
            {
                throw new NotFoundException(
                    ErrorCodes.CustomerNotFound,
                    "Customer not found.");
            }


            if (customer.Status == request.Status)
            {
                return;
            }


            var previousStatus =
                customer.Status;


            customer.Status =
                request.Status;


            customer.UpdatedAtUtc =
                DateTime.UtcNow;


            await _auditLogRepository.AddAsync(
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = "Customer",
                    EntityId = customer.Id.ToString(),
                    Action = "CustomerStatusChanged",


                    OldValues = JsonSerializer.Serialize(
                        new
                        {
                            Status =
                                previousStatus.ToString()
                        }),


                    NewValues = JsonSerializer.Serialize(
                        new
                        {
                            Status =
                                customer.Status.ToString(),
                            Reason =
                                request.Reason.Trim()
                        }),


                    PerformedBy = GetInternalActor(),


                    PerformedByRole =
                        _currentUserService.Role,


                    CorrelationId =
                        _currentUserService.CorrelationId,


                    PerformedAtUtc =
                        DateTime.UtcNow
                },
                cancellationToken);


            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }


        private static void EnsureExistingCustomerMatches(
            Customers existingCustomer,
            string firstName,
            string lastName,
            string phoneNumber)
        {
            var firstNameMatches =
                string.Equals(
                    existingCustomer.FirstName,
                    firstName,
                    StringComparison.Ordinal);


            var lastNameMatches =
                string.Equals(
                    existingCustomer.LastName,
                    lastName,
                    StringComparison.Ordinal);


            var phoneNumberMatches =
                string.Equals(
                    existingCustomer.PhoneNumber,
                    phoneNumber,
                    StringComparison.Ordinal);


            if (firstNameMatches &&
                lastNameMatches &&
                phoneNumberMatches)
            {
                return;
            }


            throw new ConflictException(
                ErrorCodes.CustomerOnboardingConflict,
                "A customer already exists for the authentication user with different details.");
        }


        private async Task AddAuditAsync(
            Guid entityId,
            string action,
            string? oldValues,
            string? newValues,
            string fallbackPerformedBy,
            CancellationToken cancellationToken)
        {
            var performedBy =
                string.IsNullOrWhiteSpace(
                    _currentUserService.UserId)
                    ? fallbackPerformedBy
                    : _currentUserService.UserId;


            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityName = "Customer",
                EntityId = entityId.ToString(),
                Action = action,
                OldValues = oldValues,
                NewValues = newValues,
                PerformedBy = performedBy,
                CorrelationId =
                    _currentUserService.CorrelationId,
                PerformedAtUtc =
                    DateTime.UtcNow
            };


            await _auditLogRepository.AddAsync(
                auditLog,
                cancellationToken);
        }




        


    }
}
