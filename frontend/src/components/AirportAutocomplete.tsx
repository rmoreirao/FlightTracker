'use client';

import { useState, useRef, useEffect } from 'react';
import { Airport, searchAirports } from '@/lib/airports';
import { MapPinIcon, ChevronDownIcon } from '@heroicons/react/24/outline';

interface AirportAutocompleteProps {
  label: string;
  placeholder: string;
  initialValue?: string;
  onChange: (code: string) => void;
  error?: string;
  icon?: React.ReactNode;
}

export function AirportAutocomplete({
  label,
  placeholder,
  initialValue,
  onChange,
  error,
  icon = <MapPinIcon className="w-4 h-4 inline mr-1" />
}: AirportAutocompleteProps) {  const [query, setQuery] = useState(initialValue || '');
  const [isOpen, setIsOpen] = useState(false);
  const [results, setResults] = useState<Airport[]>([]);
  const [highlightedIndex, setHighlightedIndex] = useState(-1);
  
  const inputRef = useRef<HTMLInputElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);
  // Search for airports when query changes
  useEffect(() => {
    console.debug('Query changed:', query, 'Length:', query.length);
    if (query.length >= 1) {
      const searchResults = searchAirports(query);
      console.debug('Search results:', searchResults.length, 'results found');
      setResults(searchResults);
      setIsOpen(searchResults.length > 0);
      setHighlightedIndex(-1);
    } else {
      console.debug('Query too short, clearing results');
      setResults([]);
      setIsOpen(false);
      setHighlightedIndex(-1);
    }
  }, [query]);  // Close dropdown when clicking outside
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node) &&
        inputRef.current &&
        !inputRef.current.contains(event.target as Node)
      ) {
        setIsOpen(false);
        setHighlightedIndex(-1);
      }
    }

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newQuery = e.target.value;
    console.debug('Input changed:', newQuery);
    setQuery(newQuery);
    
    // If user clears the input, clear the selection
    if (newQuery === '') {
      console.debug('Clearing selection');
      onChange('');
    }
  };

  const handleInputFocus = () => {
    if (results.length > 0) {
      setIsOpen(true);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (!isOpen) return;

    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault();
        setHighlightedIndex(prev => 
          prev < results.length - 1 ? prev + 1 : prev
        );
        break;
      case 'ArrowUp':
        e.preventDefault();
        setHighlightedIndex(prev => prev > 0 ? prev - 1 : prev);
        break;
      case 'Enter':
        e.preventDefault();
        if (highlightedIndex >= 0 && results[highlightedIndex]) {
          selectAirport(results[highlightedIndex]);
        }
        break;
      case 'Escape':
        setIsOpen(false);
        setHighlightedIndex(-1);
        inputRef.current?.blur();
        break;
    }
  };  const selectAirport = (airport: Airport) => {
    console.debug('Selecting airport:', airport);
    setQuery(`${airport.code} - ${airport.name}, ${airport.city}`);
    setIsOpen(false);
    setHighlightedIndex(-1);
    onChange(airport.code);
  };

  const handleInputClick = () => {
    if (results.length > 0) {
      setIsOpen(true);
    }
  };

  return (
    <div className="relative">
      <label className="block text-sm font-medium text-neutral-900 dark:text-neutral-900 mb-2">
        {icon}
        {label}
      </label>
      
      <div className="relative">
        <input
          ref={inputRef}
          type="text"
          value={query}
          onChange={handleInputChange}
          onFocus={handleInputFocus}
          onKeyDown={handleKeyDown}
          onClick={handleInputClick}
          placeholder={placeholder}
          className="w-full px-3 py-2 pr-8 border border-neutral-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
          autoComplete="off"
        />
        <ChevronDownIcon 
          className={`absolute right-2 top-1/2 transform -translate-y-1/2 w-4 h-4 text-neutral-400 transition-transform ${
            isOpen ? 'rotate-180' : ''
          }`}
        />
      </div>

      {error && (
        <p className="mt-1 text-sm text-error-500">{error}</p>
      )}

      {isOpen && results.length > 0 && (
        <div
          ref={dropdownRef}
          className="absolute z-50 w-full mt-1 bg-white border border-neutral-300 rounded-md shadow-lg max-h-60 overflow-auto"
        >
          {results.map((airport, index) => (
            <div
              key={airport.code}
              className={`px-3 py-2 cursor-pointer transition-colors ${
                index === highlightedIndex
                  ? 'bg-primary-50 text-primary-900'
                  : 'hover:bg-neutral-50'
              }`}
              onClick={() => selectAirport(airport)}
              onMouseEnter={() => setHighlightedIndex(index)}
            >
              <div className="flex items-center justify-between">
                <div>
                  <div className="font-medium text-neutral-900">
                    <span className="text-primary-600 font-bold">{airport.code}</span>
                    {' - '}
                    {airport.name}
                  </div>
                  <div className="text-sm text-neutral-600">
                    {airport.city}, {airport.country}
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
