import { Component, OnInit, inject, signal } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { SearchService } from '../../../core/services/search.service';
import { RecentSearchService } from '../../../core/services/recent-search.service';
import { SearchResult, SearchScope } from '../../../core/models/search.model';
import { HighlightPipe } from '../../../shared/pipes/highlight.pipe';

@Component({
  selector: 'app-global-search',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatPaginatorModule
  ],
  templateUrl: './global-search.component.html',
  styleUrl: './global-search.component.scss'
})
export class GlobalSearchComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private searchService = inject(SearchService);
  private recentSearchService = inject(RecentSearchService);
  private sanitizer = inject(DomSanitizer);
  private highlightPipe = new HighlightPipe();

  results = signal<SearchResult[]>([]);
  totalCount = signal(0);
  pageNumber = signal(1);
  pageSize = signal(20);
  loading = signal(false);
  recentSearches = signal<string[]>([]);

  searchForm = this.fb.group({
    q: [''],
    scope: ['all' as SearchScope]
  });

  ngOnInit(): void {
    this.recentSearches.set(this.recentSearchService.getRecent());

    this.route.queryParamMap.subscribe(params => {
      const q = params.get('q') ?? '';
      const scope = (params.get('type') as SearchScope) ?? 'all';
      this.searchForm.patchValue({ q, scope: ['all', 'projects', 'tasks', 'bugs'].includes(scope) ? scope : 'all' });
      if (q.length >= 2) this.runSearch(1);
    });
  }

  runSearch(page = 1): void {
    const { q, scope } = this.searchForm.getRawValue();
    const term = q?.trim() ?? '';
    if (term.length < 2) return;

    this.pageNumber.set(page);
    this.loading.set(true);
    this.recentSearchService.add(term);
    this.recentSearches.set(this.recentSearchService.getRecent());

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { q: term, type: scope },
      queryParamsHandling: 'merge'
    });

    this.searchService.search({ q: term, pageNumber: page, pageSize: this.pageSize() }, scope ?? 'all').subscribe({
      next: res => {
        this.results.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onPage(event: PageEvent): void {
    this.pageSize.set(event.pageSize);
    this.runSearch(event.pageIndex + 1);
  }

  applyRecent(term: string): void {
    this.searchForm.patchValue({ q: term });
    this.runSearch(1);
  }

  resultLink(item: SearchResult): string[] {
    if (item.type === 'Project') return ['/app/projects', String(item.id), 'dashboard'];
    if (item.type === 'Task') return ['/app/projects', String(item.projectId), 'tasks', String(item.id)];
    return ['/app/projects', String(item.projectId), 'bugs', String(item.id)];
  }

  typeIcon(type: string): string {
    if (type === 'Task') return 'assignment';
    if (type === 'Bug') return 'bug_report';
    return 'folder';
  }

  highlightHtml(text: string): SafeHtml {
    const term = this.searchForm.value.q ?? '';
    return this.sanitizer.bypassSecurityTrustHtml(this.highlightPipe.transform(text, term));
  }
}
