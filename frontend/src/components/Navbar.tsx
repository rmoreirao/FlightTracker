'use client';

import { useState } from 'react';
import { Bars3Icon, XMarkIcon } from '@heroicons/react/24/outline';

export function Navbar() {
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  const toggleMenu = () => setIsMenuOpen(!isMenuOpen);

  return (
    <nav className="sticky top-0 z-50 bg-neutral-50/80 dark:bg-neutral-50/80 backdrop-blur-md border-b border-neutral-200 dark:border-neutral-200">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-14">
          {/* Logo */}
          <div className="flex-shrink-0">
            <h1 className="text-xl font-bold text-neutral-900 dark:text-neutral-900">
              FlightTracker
            </h1>
          </div>

          {/* Desktop Navigation */}
          <div className="hidden md:block">
            <div className="ml-10 flex items-baseline space-x-4">
              <a
                href="#"
                className="text-neutral-700 dark:text-neutral-700 hover:text-primary-500 px-3 py-2 rounded-md text-sm font-medium transition-colors"
              >
                Search
              </a>
              <a
                href="#"
                className="text-neutral-700 dark:text-neutral-700 hover:text-primary-500 px-3 py-2 rounded-md text-sm font-medium transition-colors"
              >
                History
              </a>
              <a
                href="#"
                className="text-neutral-700 dark:text-neutral-700 hover:text-primary-500 px-3 py-2 rounded-md text-sm font-medium transition-colors"
              >
                About
              </a>
              <button className="bg-primary-500 hover:bg-primary-600 text-white px-4 py-2 rounded-md text-sm font-medium transition-colors">
                Sign In
              </button>
            </div>
          </div>

          {/* Mobile menu button */}
          <div className="md:hidden">
            <button
              onClick={toggleMenu}
              className="text-neutral-700 dark:text-neutral-700 hover:text-primary-500 p-2 rounded-md transition-colors"
              aria-label="Toggle menu"
            >
              {isMenuOpen ? (
                <XMarkIcon className="h-6 w-6" />
              ) : (
                <Bars3Icon className="h-6 w-6" />
              )}
            </button>
          </div>
        </div>

        {/* Mobile Navigation */}
        {isMenuOpen && (
          <div className="md:hidden">
            <div className="px-2 pt-2 pb-3 space-y-1 sm:px-3 border-t border-neutral-200 dark:border-neutral-200">
              <a
                href="#"
                className="text-neutral-700 dark:text-neutral-700 hover:text-primary-500 block px-3 py-2 rounded-md text-base font-medium transition-colors"
              >
                Search
              </a>
              <a
                href="#"
                className="text-neutral-700 dark:text-neutral-700 hover:text-primary-500 block px-3 py-2 rounded-md text-base font-medium transition-colors"
              >
                History
              </a>
              <a
                href="#"
                className="text-neutral-700 dark:text-neutral-700 hover:text-primary-500 block px-3 py-2 rounded-md text-base font-medium transition-colors"
              >
                About
              </a>
              <button className="w-full text-left bg-primary-500 hover:bg-primary-600 text-white px-3 py-2 rounded-md text-base font-medium transition-colors">
                Sign In
              </button>
            </div>
          </div>
        )}
      </div>
    </nav>
  );
}
