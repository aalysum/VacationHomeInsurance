import { DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';

export interface ClaimTimelineEntry {
  previousStatus: number;
  newStatus: number;
  comments: string;
  changedBy: string;
  changedAtUtc: string;
}

@Component({
  selector: 'app-claim-timeline',
  standalone: true,
  imports: [DatePipe],
  template: `
    <section class="timeline" aria-labelledby="claim-timeline-heading">
      <div class="timeline-heading">
        <div>
          <p class="timeline-eyebrow">Claim progress</p>
          <h3 id="claim-timeline-heading">Timeline</h3>
          <p class="timeline-description">
            Status changes and information requests for this claim.
          </p>
        </div>
        <span class="timeline-count">{{ entries().length }}</span>
      </div>

      @if (entries().length === 0) {
        <p class="timeline-empty">No timeline entries are available.</p>
      } @else {
        <ol class="timeline-list">
          @for (entry of entries(); track $index) {
            <li class="timeline-item">
              <span class="timeline-marker" aria-hidden="true"></span>
              <div class="timeline-content">
                <div class="timeline-meta">
                  <strong>{{ statusLabel(entry.newStatus) }}</strong>
                  <time [attr.datetime]="entry.changedAtUtc">
                    {{ entry.changedAtUtc | date: 'medium' }}
                  </time>
                </div>
                @if (entry.previousStatus !== entry.newStatus) {
                  <p class="timeline-transition">
                    {{ statusLabel(entry.previousStatus) }} to
                    {{ statusLabel(entry.newStatus) }}
                  </p>
                }
                @if (entry.comments) {
                  <p class="timeline-comment">{{ entry.comments }}</p>
                }
              </div>
            </li>
          }
        </ol>
      }
    </section>
  `,
  styles: `
    :host {
      display: block;
    }

    .timeline {
      border-top: 1px solid #dce5ea;
      padding-top: 1.5rem;
    }

    .timeline-heading,
    .timeline-meta {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 1rem;
    }

    .timeline-eyebrow {
      margin: 0;
      color: #6f8598;
      font-size: 0.6875rem;
      font-weight: 600;
      letter-spacing: 0.12em;
      text-transform: uppercase;
    }

    h3 {
      margin: 0.35rem 0 0;
      color: #0b2545;
      font-size: 1.125rem;
      font-weight: 600;
    }

    .timeline-description,
    .timeline-empty {
      margin: 0.5rem 0 0;
      color: #748799;
      font-size: 0.875rem;
      line-height: 1.5;
    }

    .timeline-count {
      min-width: 2rem;
      padding: 0.25rem 0.5rem;
      border: 1px solid #d7e1e7;
      border-radius: 999px;
      color: #52697d;
      font-size: 0.75rem;
      text-align: center;
    }

    .timeline-list {
      display: grid;
      gap: 0;
      margin: 1.25rem 0 0;
      padding: 0;
      list-style: none;
    }

    .timeline-item {
      position: relative;
      display: grid;
      grid-template-columns: 1rem minmax(0, 1fr);
      gap: 0.75rem;
      padding-bottom: 1.25rem;
    }

    .timeline-item:not(:last-child)::before {
      position: absolute;
      top: 0.75rem;
      bottom: 0;
      left: 0.45rem;
      width: 1px;
      background: #dce5ea;
      content: '';
    }

    .timeline-marker {
      z-index: 1;
      width: 0.875rem;
      height: 0.875rem;
      margin-top: 0.2rem;
      border: 2px solid #fff;
      border-radius: 50%;
      background: #39745c;
      box-shadow: 0 0 0 1px #a9c8b8;
    }

    .timeline-content {
      min-width: 0;
    }

    .timeline-meta strong {
      color: #193a5c;
      font-size: 0.875rem;
    }

    time {
      color: #8293a2;
      font-size: 0.75rem;
    }

    .timeline-transition,
    .timeline-comment {
      margin: 0.45rem 0 0;
      color: #52697d;
      font-size: 0.875rem;
      line-height: 1.5;
      overflow-wrap: anywhere;
      white-space: pre-wrap;
    }

    @media (max-width: 520px) {
      .timeline-meta {
        flex-direction: column;
        gap: 0.25rem;
      }
    }
  `
})
export class ClaimTimelineComponent {
  readonly entries = input.required<readonly ClaimTimelineEntry[]>();

  statusLabel(status: number): string {
    return [
      'Unknown',
      'Submitted',
      'Under review',
      'Additional information requested',
      'Approved',
      'Rejected',
      'Paid',
      'Closed',
      'Withdrawn'
    ][status] ?? 'Unknown';
  }
}
