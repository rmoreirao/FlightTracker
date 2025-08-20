'use client';

import { Navbar } from '@/components/Navbar';
import { FlightSearchWrapper } from '@/components/FlightSearchWrapper';

export default function Home() {
  return (
    <div className="min-h-screen bg-neutral-50 dark:bg-neutral-50">
      <Navbar />
      
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <FlightSearchWrapper />
      </main>
    </div>
  );
}
