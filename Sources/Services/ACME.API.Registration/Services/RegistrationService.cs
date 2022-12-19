#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ACME.API.Registration.Exceptions;
using ACME.API.Registration.Models;
using ACME.API.Registration.Repositories.Interfaces;
using ACME.API.Registration.Services.Interfaces;
using ACME.API.Registration.Validators;
using ACME.Library.Domain.Enums.Registration;
using ACME.Library.Domain.Registration;
using ACME.Library.Saga.Abstractions;
using FluentValidation.Results;

namespace ACME.API.Registration.Services
{
    public class RegistrationService : IRegistrationService, ISagaStateRepository<RegistrationData>
    {
        private readonly IRegistrationRepository _registrationRepository;
        private readonly IQueueRegistrationService _queueRegistrationService;

        public RegistrationService(IRegistrationRepository registrationRepository, IQueueRegistrationService queueRegistrationService)
        {
            _registrationRepository = registrationRepository;
            _queueRegistrationService = queueRegistrationService;
        }

        public async Task<RegistrationData> AddRegistration(RegistrationData data)
        {
            var existing = await _registrationRepository.GetSingleAsync(x => x.Email.ToLower() == data.Email.ToLower());

            if (existing != null)
            {
                var errors = new List<ValidationFailure>()
                {
                    new ValidationFailure("CompanyName", $"The Email {data.Email} already exists in the registration data.")
                    {
                        ErrorCode = ValidatorConstants.RegistrationExists
                    }
                };
                
                Console.WriteLine($"The Email {data.Email} already exists in the registration data.");
                throw new ValidationFailedException(errors);
            }

            if(data.RegistrationState == RegistrationStateType.None)
            {
                data.RegistrationState = RegistrationStateType.Started;
            }

            var container = await _registrationRepository.AddRegistration(data);

            if (data.RegistrationState == RegistrationStateType.Submitted)
            {
                await _queueRegistrationService.SubmitRegistration(container);
            }

            return container;
        }

        public async Task UpdateRegistration(RegistrationData data)
        {
            await UpdateRegistrationDataAsync(data);

            switch (data.Action)
            {
                case RegistrationActionType.Submit:
                    await _queueRegistrationService.SubmitRegistration(data);
                    break;
                case RegistrationActionType.None:
                default:
                    break;
            }
        }


        private async Task UpdateRegistrationDataAsync(RegistrationData data)
        {
            switch (data.Action)
            {
                case RegistrationActionType.ExternalSigning:
                    data.RegistrationState = RegistrationStateType.AwaitSigningOther;
                    break;
                case RegistrationActionType.Submit:
                    data.RegistrationState = RegistrationStateType.Submitted;
                    break;
                case RegistrationActionType.BackofficeApprove:
                    data.RegistrationState = RegistrationStateType.Approved;
                    break;
                case RegistrationActionType.BackofficeReject:
                    data.RegistrationState = RegistrationStateType.Rejected;
                    break;
                case RegistrationActionType.None:
                case RegistrationActionType.Save:
                default:
                    data.RegistrationState = RegistrationStateType.Started;
                    break;
            }

            await _registrationRepository.UpdateAsync(data);
        }

        public async Task<RegistrationData> GetRegistration(Guid correlationId)
        {
            var registration = await _registrationRepository.GetRegistrationByCorrelationId(correlationId);
            if (registration == null)
            {
                throw new NotFoundException<RegistrationData>(correlationId.ToString());
            }

            return registration;
        }

        public async Task<RegistrationData> GetRegistration(RegistrationVerificationModel verificationModel)
        {
            var registration = await _registrationRepository.GetRegistrationByCorrelationId(verificationModel.CorrelationId);
            if (registration == null)
            {
                throw new NotFoundException<RegistrationData>(verificationModel.CorrelationId.ToString());
            }

            // //TODO Dit moet nog wel aangepast worden, nu werkt verificatie op elke contact in registratie niet specifiek genoeg
            // if (registration.Contacts.Any() && registration.Contacts.FirstOrDefault(x => x.Email == verificationModel.Email) == null)
            // {
            //     throw new ValidationFailedException(new List<ValidationFailure>()
            //     {
            //         new ValidationFailure("email", "Email address verification failed.")
            //         {
            //             ErrorCode = ValidatorConstants.EmailForValidationInvalid
            //         }
            //     });
            // }

            return registration;
        }

        public RegistrationData GetByCorrelationId(Guid correlationId)
        {
            return _registrationRepository.GetRegistrationByCorrelationId(correlationId).Result;
        }

        public async Task CreateAsync(RegistrationData registrationData)
        {
            await AddRegistration(registrationData);
        }

        public async Task UpdateAsync(RegistrationData registrationData)
        {
            await UpdateRegistration(registrationData);
        }

        public async Task SaveChangesAsync()
        {
            await _registrationRepository.SaveChangesAsync();
        }

        
    }
}