// Test file to verify API integration setup
import { flightApi } from '../src/lib/api';
import { FlightSearchParams } from '../src/lib/api-types';

async function testApiIntegration() {
  try {
    console.log('Testing FlightTracker API integration...');
    
    // Test health endpoint first
    console.log('1. Testing health endpoint...');
    const health = await flightApi.getHealthStatus();
    console.log('Health check result:', health);
    
    // Test flight search
    console.log('2. Testing flight search...');
    const searchParams: FlightSearchParams = {
      originCode: 'LAX',
      destinationCode: 'JFK',
      departureDate: '2025-07-01',
      adults: 1,
      cabins: 'Economy'
    };
    
    const searchResults = await flightApi.searchFlights(searchParams);
    console.log('Search results:', searchResults);
    
    console.log('✅ API integration test completed successfully!');
  } catch (error) {
    console.error('❌ API integration test failed:', error);
  }
}

// Uncomment to run the test
// testApiIntegration();

export { testApiIntegration };
