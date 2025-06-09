'use client';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Client, Provider as UrqlProvider, cacheExchange, fetchExchange } from 'urql';
import React from 'react';

// Create a new QueryClient instance
const queryClient = new QueryClient();

// Create a new urql client instance
const urqlClient = new Client({
  url: '/api/graphql', // Replace with your GraphQL endpoint
  exchanges: [cacheExchange, fetchExchange],
});

export function Providers({ children }: { children: React.ReactNode }) {
  return (
    <QueryClientProvider client={queryClient}>
      <UrqlProvider value={urqlClient}>
        {children}
      </UrqlProvider>
    </QueryClientProvider>
  );
}
