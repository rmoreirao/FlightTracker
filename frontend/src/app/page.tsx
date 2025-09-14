'use client';

import { Navbar } from '@/components/Navbar';
import { FlightSearchWrapper } from '@/components/FlightSearchWrapper';
import { ItinerarySearchWrapper } from '@/components/ItinerarySearchWrapper';

const USE_ITIN = process.env.NEXT_PUBLIC_USE_ITINERARIES === 'true';

export default function Home() {
  return (
    <div className="min-h-screen bg-neutral-50 dark:bg-neutral-50">
      <Navbar />
      
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {USE_ITIN ? <ItinerarySearchWrapper /> : <FlightSearchWrapper />}
      </main>
    </div>
  );
}
