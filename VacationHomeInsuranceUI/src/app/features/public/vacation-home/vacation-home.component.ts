import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CoverageType } from '../../../core/models/customer/quotes/coverage-type.enum';
import { ProductDetails } from '../../../core/models/customer/quotes/product-details.model';
import { ProductCoverage } from '../../../core/models/customer/quotes/product-coverage.model';
import { CustomerQuoteService } from '../../../core/services/customer-quote.service';

interface CoverageGroup {
  type: CoverageType;
  label: string;
}

@Component({
  selector: 'app-vacation-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './vacation-home.component.html',
  styleUrl: './vacation-home.component.css'
})
export class VacationHomeComponent implements OnInit {
  private readonly quoteService = inject(CustomerQuoteService);
  private readonly router = inject(Router);

  readonly coverageGroups: readonly CoverageGroup[] = [
    { type: CoverageType.Mandatory, label: 'Included coverages' },
    { type: CoverageType.Optional, label: 'Optional coverages' }
  ];

  readonly products = computed(() =>
    this.quoteService.products().filter(product => product.isActive)
  );
  readonly loading = this.quoteService.productsLoading;
  readonly errorMessage = this.quoteService.errorMessage;

  ngOnInit(): void {
    this.quoteService.loadProducts();
  }

  coveragesFor(
    product: ProductDetails,
    type: CoverageType
  ): ProductCoverage[] {
    return product.coverages.filter(coverage => coverage.coverageType === type);
  }

  startQuote(): void {
    void this.router.navigate(['/auth/login'], {
      queryParams: { returnUrl: '/customer/quotes?action=generate' }
    });
  }
}