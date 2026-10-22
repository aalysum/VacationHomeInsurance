import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';


interface InsuranceProduct {
  name: string;
  description: string;
  available: boolean;
}


interface InsuranceDomain {
  id: string;
  number: string;
  name: string;
  shortName: string;
  description: string;
  icon: string;
  available: boolean;
  accentClass: string;
  products: InsuranceProduct[];
}


@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.css'
})
export class LandingComponent {
  readonly currentYear = new Date().getFullYear();


  selectedDomainId = 'home';


  readonly domains: InsuranceDomain[] = [
    {
      id: 'home',
      number: '01',
      name: 'Home Insurance',
      shortName: 'Home',
      description:
        'Protection designed for residential properties, belongings, and the places you call home.',
      icon: 'home',
      available: true,
      accentClass: 'from-[#DCE8F1] to-[#F5F9FC]',
      products: [
        {
          name: 'Condo Insurance',
          description: 'Protection designed for condominium owners.',
          available: false
        },
        {
          name: 'Commercial Property Insurance',
          description: 'Coverage for eligible commercial properties.',
          available: false
        },
        {
          name: 'Vacation Home Insurance',
          description: 'Protection for seasonal and secondary residences.',
          available: true
        },
        {
          name: 'Renters Insurance',
          description: 'Protection for personal belongings in rented homes.',
          available: false
        },
        {
          name: 'House Owner Insurance',
          description: 'Comprehensive protection for homeowners.',
          available: false
        }
      ]
    },
    {
      id: 'business',
      number: '02',
      name: 'Business Insurance',
      shortName: 'Business',
      description:
        'Insurance solutions created to support organizations, employees, and business continuity.',
      icon: 'business',
      available: false,
      accentClass: 'from-[#E3EBF3] to-[#F8FAFC]',
      products: [
        {
          name: 'Business Owners Policy',
          description: 'Combined protection for eligible business owners.',
          available: false
        },
        {
          name: 'Business Interruption Coverage',
          description: 'Support for disruptions affecting business operations.',
          available: false
        },
        {
          name: 'Commercial Property Insurance',
          description: 'Protection for business buildings and property.',
          available: false
        },
        {
          name: 'Workers Compensation',
          description: 'Coverage supporting workplace-related incidents.',
          available: false
        },
        {
          name: 'Employment Practices Liability Insurance',
          description: 'Protection for eligible employment-related risks.',
          available: false
        }
      ]
    },
    {
      id: 'auto',
      number: '03',
      name: 'Auto Insurance',
      shortName: 'Auto',
      description:
        'Coverage for everyday vehicles, recreational transport, and emerging mobility needs.',
      icon: 'auto',
      available: false,
      accentClass: 'from-[#DDE8EE] to-[#F6FAFB]',
      products: [
        {
          name: 'Car Insurance',
          description: 'Protection for personal vehicles.',
          available: false
        },
        {
          name: 'Recreational Vehicle Insurance',
          description: 'Coverage for eligible recreational vehicles.',
          available: false
        },
        {
          name: 'Snowmobile Insurance',
          description: 'Protection for eligible snowmobiles.',
          available: false
        },
        {
          name: 'Transport Network Company Insurance',
          description: 'Coverage for eligible transportation networks.',
          available: false
        },
        {
          name: 'Electric Vehicle Insurance',
          description: 'Modern protection for electric vehicles.',
          available: false
        }
      ]
    },
    {
      id: 'disaster',
      number: '04',
      name: 'Disaster Insurance',
      shortName: 'Disaster',
      description:
        'Focused protection for properties exposed to severe weather and natural disasters.',
      icon: 'disaster',
      available: false,
      accentClass: 'from-[#E5E9F0] to-[#FAFBFC]',
      products: [
        {
          name: 'Wildfire Insurance',
          description: 'Protection against eligible wildfire damage.',
          available: false
        },
        {
          name: 'Earthquake',
          description: 'Coverage for eligible earthquake-related damage.',
          available: false
        },
        {
          name: 'Flood Insurance',
          description: 'Protection against eligible flood-related loss.',
          available: false
        },
        {
          name: 'Hailstorm Insurance',
          description: 'Coverage for eligible hailstorm damage.',
          available: false
        },
        {
          name: 'Hurricane Insurance',
          description: 'Protection against eligible hurricane damage.',
          available: false
        }
      ]
    },
    {
      id: 'employee-benefits',
      number: '05',
      name: 'Employee Benefits',
      shortName: 'Benefits',
      description:
        'Benefit solutions that support employees, groups, associations, and organizations.',
      icon: 'benefits',
      available: false,
      accentClass: 'from-[#E2ECE9] to-[#F7FAF9]',
      products: [
        {
          name: 'Sports and Sports Camp Insurance',
          description: 'Protection for eligible sports and camp activities.',
          available: false
        },
        {
          name: 'Association and Affinity Insurance',
          description: 'Coverage for eligible associations and groups.',
          available: false
        },
        {
          name: 'Group Life Insurance',
          description: 'Life protection for eligible employee groups.',
          available: false
        },
        {
          name: 'Health and Accident Insurance',
          description: 'Support for eligible health and accident risks.',
          available: false
        },
        {
          name: 'Group Disability Insurance',
          description: 'Disability protection for eligible employee groups.',
          available: false
        }
      ]
    }
  ];


  constructor(private readonly router: Router) {}


  get selectedDomain(): InsuranceDomain {
    return (
      this.domains.find(domain => domain.id === this.selectedDomainId) ??
      this.domains[0]
    );
  }


  selectDomain(domainId: string): void {
    this.selectedDomainId = domainId;
  }


  scrollToSection(sectionId: string): void {
    document
      .getElementById(sectionId)
      ?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }


  openProduct(product: InsuranceProduct): void {
    if (!product.available) {
      return;
    }

    if (product.name === 'Vacation Home Insurance') {
      void this.router.navigate([
        '/products/vacation-home-insurance'
      ]);
    }
  }


  generateQuote(): void {
    this.router.navigate(['/auth/login'], {
      queryParams: {
        returnUrl: '/customer/quotes?action=generate'
      }
    });
  }


  register(): void {
    this.router.navigate(['/auth/register/customer']);
  }
}
