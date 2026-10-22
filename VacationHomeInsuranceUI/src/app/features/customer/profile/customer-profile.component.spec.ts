import {
  signal
} from '@angular/core';


import {
  ComponentFixture,
  TestBed
} from '@angular/core/testing';


import {
  Router
} from '@angular/router';


import {
  beforeEach,
  describe,
  expect,
  it,
  vi
} from 'vitest';


import {
  CustomerStatus
} from '../../../core/models/admin/customer-status.enum';


import {
  AuthService
} from '../../../core/services/auth.service';


import {
  CustomerService
} from '../../../core/services/customer.service';


import {
  CustomerProfileComponent
} from './customer-profile.component';


describe(
  'CustomerProfileComponent',
  () => {
    let component:
      CustomerProfileComponent;


    let fixture:
      ComponentFixture<
        CustomerProfileComponent
      >;


    const profileSignal =
      signal<any>(
        null
      );


    const loadingSignal =
      signal(
        false
      );


    const errorMessageSignal =
      signal<string | null>(
        null
      );


    const successMessageSignal =
      signal<string | null>(
        null
      );


    const customerServiceMock = {
      profile:
        profileSignal.asReadonly(),


      profileLoading:
        loadingSignal.asReadonly(),


      errorMessage:
        errorMessageSignal.asReadonly(),


      successMessage:
        successMessageSignal.asReadonly(),


      clearError:
        vi.fn(),


      clearSuccessMessage:
        vi.fn(),


      loadProfile:
        vi.fn(),


      updateProfile:
        vi.fn()
    };


    const authServiceMock = {
      logout:
        vi.fn()
    };


    const routerMock = {
      url:
        '/customer/profile',


      navigateByUrl:
        vi.fn()
          .mockResolvedValue(
            true
          )
    };


    const customerProfile = {
      firstName:
        'Swapanthi',


      lastName:
        'Bommidi',


      phoneNumber:
        '9876543210',


      status:
        CustomerStatus.Active,


      dateOfBirth:
        '2000-05-15T00:00:00',


      addressLine1:
        'Road Number 10',


      addressLine2:
        'Banjara Hills',


      city:
        'Hyderabad',


      state:
        'Telangana',


      postalCode:
        '500034',


      country:
        'India'
    };


    beforeEach(
      async () => {
        profileSignal.set(
          null
        );


        loadingSignal.set(
          false
        );


        errorMessageSignal.set(
          null
        );


        successMessageSignal.set(
          null
        );


        vi.clearAllMocks();


        routerMock.url =
          '/customer/profile';


        routerMock.navigateByUrl
          .mockResolvedValue(
            true
          );


        await TestBed
          .configureTestingModule({
            imports: [
              CustomerProfileComponent
            ],


            providers: [
              {
                provide:
                  CustomerService,


                useValue:
                  customerServiceMock
              },


              {
                provide:
                  AuthService,


                useValue:
                  authServiceMock
              },


              {
                provide:
                  Router,


                useValue:
                  routerMock
              }
            ]
          })
          .overrideComponent(
            CustomerProfileComponent,
            {
              set: {
                template:
                  ''
              }
            }
          )
          .compileComponents();


        fixture =
          TestBed.createComponent(
            CustomerProfileComponent
          );


        component =
          fixture.componentInstance;
      }
    );


    it(
      'should create',
      () => {
        fixture.detectChanges();


        expect(
          component
        ).toBeTruthy();
      }
    );


    it(
      'should clear messages and load the profile on initialization',
      () => {
        fixture.detectChanges();


        expect(
          customerServiceMock.clearError
        ).toHaveBeenCalledOnce();


        expect(
          customerServiceMock
            .clearSuccessMessage
        ).toHaveBeenCalledOnce();


        expect(
          customerServiceMock.loadProfile
        ).toHaveBeenCalledOnce();
      }
    );


    it(
      'should populate the form from the profile',
      () => {
        profileSignal.set(
          customerProfile
        );


        component.populateForm();


        expect(
          component.profileForm
            .getRawValue()
        ).toEqual({
          dateOfBirth:
            '2000-05-15',


          addressLine1:
            'Road Number 10',


          addressLine2:
            'Banjara Hills',


          city:
            'Hyderabad',


          state:
            'Telangana',


          postalCode:
            '500034',


          country:
            'India'
        });
      }
    );


    it(
      'should leave the form unchanged when the profile is unavailable',
      () => {
        profileSignal.set(
          null
        );


        component.profileForm
          .patchValue({
            city:
              'Mumbai'
          });


        component.populateForm();


        expect(
          component.profileForm.controls
            .city
            .value
        ).toBe(
          'Mumbai'
        );
      }
    );


    it(
      'should submit a trimmed profile update request',
      () => {
        component.profileForm
          .setValue({
            dateOfBirth:
              '2000-05-15',


            addressLine1:
              '  Road Number 10  ',


            addressLine2:
              '  Banjara Hills  ',


            city:
              '  Hyderabad  ',


            state:
              '  Telangana  ',


            postalCode:
              '  500034  ',


            country:
              '  India  '
          });


        component.saveProfile();


        expect(
          customerServiceMock.updateProfile
        ).toHaveBeenCalledOnce();


        expect(
          customerServiceMock.updateProfile
        ).toHaveBeenCalledWith({
          dateOfBirth:
            '2000-05-15',


          addressLine1:
            'Road Number 10',


          addressLine2:
            'Banjara Hills',


          city:
            'Hyderabad',


          state:
            'Telangana',


          postalCode:
            '500034',


          country:
            'India'
        });
      }
    );


    it(
      'should convert an empty date of birth to null',
      () => {
        component.profileForm
          .setValue({
            dateOfBirth:
              '',


            addressLine1:
              '',


            addressLine2:
              '',


            city:
              '',


            state:
              '',


            postalCode:
              '',


            country:
              ''
          });


        component.saveProfile();


        expect(
          customerServiceMock.updateProfile
        ).toHaveBeenCalledWith({
          dateOfBirth:
            null,


          addressLine1:
            '',


          addressLine2:
            '',


          city:
            '',


          state:
            '',


          postalCode:
            '',


          country:
            ''
        });
      }
    );


    it(
      'should not submit an invalid form',
      () => {
        component.profileForm.controls
          .addressLine1
          .setValue(
            'A'.repeat(
              251
            )
          );


        component.saveProfile();


        expect(
          component.profileForm.invalid
        ).toBe(
          true
        );


        expect(
          customerServiceMock.updateProfile
        ).not.toHaveBeenCalled();
      }
    );


    it(
      'should not submit while the profile is loading',
      () => {
        loadingSignal.set(
          true
        );


        component.profileForm
          .patchValue({
            city:
              'Hyderabad'
          });


        component.saveProfile();


        expect(
          customerServiceMock.updateProfile
        ).not.toHaveBeenCalled();
      }
    );


    it(
      'should return the Customer display name and initials',
      () => {
        profileSignal.set(
          customerProfile
        );


        expect(
          component.displayName()
        ).toBe(
          'Swapanthi Bommidi'
        );


        expect(
          component.initials()
        ).toBe(
          'SB'
        );
      }
    );


    it(
      'should return the Customer phone number',
      () => {
        profileSignal.set(
          customerProfile
        );


        expect(
          component.phoneNumber()
        ).toBe(
          '9876543210'
        );
      }
    );


    it(
      'should return fallback values when the profile is unavailable',
      () => {
        profileSignal.set(
          null
        );


        expect(
          component.displayName()
        ).toBe(
          'Customer'
        );


        expect(
          component.initials()
        ).toBe(
          'CU'
        );


        expect(
          component.phoneNumber()
        ).toBe(
          'Not available'
        );


        expect(
          component.accountStatus()
        ).toBe(
          'Inactive'
        );
      }
    );


    it(
      'should return Active for an active Customer',
      () => {
        profileSignal.set(
          customerProfile
        );


        expect(
          component.accountStatus()
        ).toBe(
          'Active'
        );
      }
    );


    it(
      'should navigate to Customer properties',
      () => {
        component.openProperties();


        expect(
          routerMock.navigateByUrl
        ).toHaveBeenCalledOnce();


        expect(
          routerMock.navigateByUrl
        ).toHaveBeenCalledWith(
          '/customer/properties'
        );
      }
    );


    it(
      'should navigate to change-password settings',
      () => {
        component.openChangePassword();


        expect(
          routerMock.navigateByUrl
        ).toHaveBeenCalledOnce();


        expect(
          routerMock.navigateByUrl
        ).toHaveBeenCalledWith(
          '/account/security/change-password'
        );
      }
    );


    it(
      'should navigate to MFA settings',
      () => {
        component.openMfaSettings();


        expect(
          routerMock.navigateByUrl
        ).toHaveBeenCalledOnce();


        expect(
          routerMock.navigateByUrl
        ).toHaveBeenCalledWith(
          '/account/security/mfa'
        );
      }
    );


    it(
      'should identify the current navigation item',
      () => {
        const profileItem =
          component.navigationItems
            .find(
              item =>
                item.route ===
                '/customer/profile'
            );


        expect(
          profileItem
        ).toBeDefined();


        expect(
          component.isCurrentNavigationItem(
            profileItem!
          )
        ).toBe(
          true
        );
      }
    );


    it(
      'should reject an unrelated navigation item',
      () => {
        const propertiesItem =
          component.navigationItems
            .find(
              item =>
                item.route ===
                '/customer/properties'
            );


        expect(
          propertiesItem
        ).toBeDefined();


        expect(
          component.isCurrentNavigationItem(
            propertiesItem!
          )
        ).toBe(
          false
        );
      }
    );


    it(
      'should clear the error message',
      () => {
        component.clearError();


        expect(
          customerServiceMock.clearError
        ).toHaveBeenCalledOnce();
      }
    );


    it(
      'should clear the success message',
      () => {
        component.clearSuccessMessage();


        expect(
          customerServiceMock
            .clearSuccessMessage
        ).toHaveBeenCalledOnce();
      }
    );


    it(
      'should log out',
      () => {
        component.logout();


        expect(
          authServiceMock.logout
        ).toHaveBeenCalledOnce();
      }
    );


    it(
      'should clear service messages when destroyed',
      () => {
        fixture.detectChanges();


        customerServiceMock.clearError
          .mockClear();


        customerServiceMock
          .clearSuccessMessage
          .mockClear();


        component.ngOnDestroy();


        expect(
          customerServiceMock.clearError
        ).toHaveBeenCalledOnce();


        expect(
          customerServiceMock
            .clearSuccessMessage
        ).toHaveBeenCalledOnce();
      }
    );
  }
);




