'use client';

import { ChevronLeftIcon, ChevronRightIcon } from '@heroicons/react/24/outline';
import { PaginationInfo } from '@/lib/schemas';

interface PaginationProps {
  paginationInfo: PaginationInfo;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
  isLoading?: boolean;
  className?: string;
}

export function Pagination({
  paginationInfo,
  onPageChange,
  onPageSizeChange,
  isLoading = false,
  className = '',
}: PaginationProps) {
  const {
    currentPage,
    totalPages,
    pageSize,
    totalResults,
    hasNextPage,
    hasPreviousPage,
  } = paginationInfo;

  // Calculate the range of items being displayed
  const startItem = Math.min((currentPage - 1) * pageSize + 1, totalResults);
  const endItem = Math.min(currentPage * pageSize, totalResults);

  // Generate page numbers to display
  const getPageNumbers = () => {
    const pages: (number | string)[] = [];
    const maxPagesToShow = 7;
    
    if (totalPages <= maxPagesToShow) {
      // Show all pages if total pages is small
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      // Show first page
      pages.push(1);
      
      if (currentPage > 4) {
        pages.push('...');
      }
      
      // Show current page and surrounding pages
      const start = Math.max(2, currentPage - 1);
      const end = Math.min(totalPages - 1, currentPage + 1);
      
      for (let i = start; i <= end; i++) {
        pages.push(i);
      }
      
      if (currentPage < totalPages - 3) {
        pages.push('...');
      }
      
      // Show last page
      if (totalPages > 1) {
        pages.push(totalPages);
      }
    }
    
    return pages;
  };

  const pageNumbers = getPageNumbers();

  if (totalResults === 0) {
    return null;
  }

  return (
    <div className={`flex flex-col md:flex-row md:items-center md:justify-between gap-4 ${className}`}>
      {/* Results info and page size selector */}
      <div className="flex flex-col sm:flex-row sm:items-center gap-4">
        <div className="text-sm text-neutral-600">
          Showing <span className="font-medium">{startItem}</span> to{' '}
          <span className="font-medium">{endItem}</span> of{' '}
          <span className="font-medium">{totalResults}</span> results
        </div>
        
        <div className="flex items-center gap-2 text-sm">
          <label htmlFor="pageSize" className="text-neutral-600">
            Show:
          </label>
          <select
            id="pageSize"
            value={pageSize}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
            disabled={isLoading}
            className="border border-neutral-300 rounded px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <option value={10}>10</option>
            <option value={20}>20</option>
            <option value={50}>50</option>
            <option value={100}>100</option>
          </select>
          <span className="text-neutral-600">per page</span>
        </div>
      </div>

      {/* Pagination controls */}
      <div className="flex items-center gap-1">
        {/* Previous button */}
        <button
          onClick={() => onPageChange(currentPage - 1)}
          disabled={!hasPreviousPage || isLoading}
          className="flex items-center gap-1 px-3 py-2 text-sm font-medium text-neutral-500 bg-neutral-100 border border-neutral-300 rounded-l-md hover:bg-neutral-200 hover:text-neutral-700 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:bg-neutral-100 disabled:hover:text-neutral-500"
          aria-label="Go to previous page"
        >
          <ChevronLeftIcon className="w-4 h-4" />
          <span className="hidden sm:inline">Previous</span>
        </button>

        {/* Page numbers */}
        <div className="flex">
          {pageNumbers.map((page, index) => {
            if (page === '...') {
              return (
                <span
                  key={`ellipsis-${index}`}
                  className="px-3 py-2 text-sm text-neutral-500 bg-neutral-100 border-t border-b border-neutral-300"
                >
                  ...
                </span>
              );
            }

            const pageNumber = page as number;
            const isCurrentPage = pageNumber === currentPage;

            return (
              <button
                key={pageNumber}
                onClick={() => onPageChange(pageNumber)}
                disabled={isLoading}
                className={`px-3 py-2 text-sm font-medium border-t border-b border-neutral-300 hover:bg-neutral-200 disabled:opacity-50 disabled:cursor-not-allowed ${
                  isCurrentPage
                    ? 'bg-primary-500 text-white border-primary-500 hover:bg-primary-600'
                    : 'bg-neutral-100 text-neutral-700 hover:text-neutral-900'
                }`}
                aria-label={`Go to page ${pageNumber}`}
                aria-current={isCurrentPage ? 'page' : undefined}
              >
                {pageNumber}
              </button>
            );
          })}
        </div>

        {/* Next button */}
        <button
          onClick={() => onPageChange(currentPage + 1)}
          disabled={!hasNextPage || isLoading}
          className="flex items-center gap-1 px-3 py-2 text-sm font-medium text-neutral-500 bg-neutral-100 border border-neutral-300 rounded-r-md hover:bg-neutral-200 hover:text-neutral-700 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:bg-neutral-100 disabled:hover:text-neutral-500"
          aria-label="Go to next page"
        >
          <span className="hidden sm:inline">Next</span>
          <ChevronRightIcon className="w-4 h-4" />
        </button>
      </div>
    </div>
  );
}
