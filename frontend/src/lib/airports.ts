export interface Airport {
  code: string;
  name: string;
  city: string;
  country: string;
}

export const airports: Airport[] = [
  // Major US Airports
  { code: 'LAX', name: 'Los Angeles International Airport', city: 'Los Angeles', country: 'United States' },
  { code: 'JFK', name: 'John F. Kennedy International Airport', city: 'New York', country: 'United States' },
  { code: 'LGA', name: 'LaGuardia Airport', city: 'New York', country: 'United States' },
  { code: 'EWR', name: 'Newark Liberty International Airport', city: 'Newark', country: 'United States' },
  { code: 'ORD', name: 'O\'Hare International Airport', city: 'Chicago', country: 'United States' },
  { code: 'MDW', name: 'Midway International Airport', city: 'Chicago', country: 'United States' },
  { code: 'ATL', name: 'Hartsfield-Jackson Atlanta International Airport', city: 'Atlanta', country: 'United States' },
  { code: 'MIA', name: 'Miami International Airport', city: 'Miami', country: 'United States' },
  { code: 'DFW', name: 'Dallas/Fort Worth International Airport', city: 'Dallas', country: 'United States' },
  { code: 'DEN', name: 'Denver International Airport', city: 'Denver', country: 'United States' },
  { code: 'SFO', name: 'San Francisco International Airport', city: 'San Francisco', country: 'United States' },
  { code: 'SJC', name: 'San Jose International Airport', city: 'San Jose', country: 'United States' },
  { code: 'OAK', name: 'Oakland International Airport', city: 'Oakland', country: 'United States' },
  { code: 'SEA', name: 'Seattle-Tacoma International Airport', city: 'Seattle', country: 'United States' },
  { code: 'LAS', name: 'Harry Reid International Airport', city: 'Las Vegas', country: 'United States' },
  { code: 'PHX', name: 'Sky Harbor International Airport', city: 'Phoenix', country: 'United States' },
  { code: 'BOS', name: 'Logan International Airport', city: 'Boston', country: 'United States' },
  { code: 'DTW', name: 'Detroit Metropolitan Wayne County Airport', city: 'Detroit', country: 'United States' },
  { code: 'MSP', name: 'Minneapolis-Saint Paul International Airport', city: 'Minneapolis', country: 'United States' },
  { code: 'CLT', name: 'Charlotte Douglas International Airport', city: 'Charlotte', country: 'United States' },
  { code: 'IAH', name: 'George Bush Intercontinental Airport', city: 'Houston', country: 'United States' },
  { code: 'HOU', name: 'William P. Hobby Airport', city: 'Houston', country: 'United States' },
  { code: 'PHL', name: 'Philadelphia International Airport', city: 'Philadelphia', country: 'United States' },
  { code: 'DCA', name: 'Ronald Reagan Washington National Airport', city: 'Washington', country: 'United States' },
  { code: 'IAD', name: 'Washington Dulles International Airport', city: 'Washington', country: 'United States' },
  { code: 'BWI', name: 'Baltimore/Washington International Airport', city: 'Baltimore', country: 'United States' },
  { code: 'SAN', name: 'San Diego International Airport', city: 'San Diego', country: 'United States' },
  { code: 'TPA', name: 'Tampa International Airport', city: 'Tampa', country: 'United States' },
  { code: 'MCO', name: 'Orlando International Airport', city: 'Orlando', country: 'United States' },
  { code: 'FLL', name: 'Fort Lauderdale-Hollywood International Airport', city: 'Fort Lauderdale', country: 'United States' },
  { code: 'PDX', name: 'Portland International Airport', city: 'Portland', country: 'United States' },
  { code: 'SLC', name: 'Salt Lake City International Airport', city: 'Salt Lake City', country: 'United States' },
  { code: 'RDU', name: 'Raleigh-Durham International Airport', city: 'Raleigh', country: 'United States' },
  { code: 'AUS', name: 'Austin-Bergstrom International Airport', city: 'Austin', country: 'United States' },
  { code: 'BNA', name: 'Nashville International Airport', city: 'Nashville', country: 'United States' },
  { code: 'STL', name: 'Lambert-St. Louis International Airport', city: 'St. Louis', country: 'United States' },
  { code: 'MSY', name: 'Louis Armstrong New Orleans International Airport', city: 'New Orleans', country: 'United States' },
  { code: 'CLE', name: 'Cleveland Hopkins International Airport', city: 'Cleveland', country: 'United States' },
  { code: 'CMH', name: 'John Glenn Columbus International Airport', city: 'Columbus', country: 'United States' },
  { code: 'IND', name: 'Indianapolis International Airport', city: 'Indianapolis', country: 'United States' },
  { code: 'MKE', name: 'Milwaukee Mitchell International Airport', city: 'Milwaukee', country: 'United States' },
  { code: 'PIT', name: 'Pittsburgh International Airport', city: 'Pittsburgh', country: 'United States' },
  { code: 'CVG', name: 'Cincinnati/Northern Kentucky International Airport', city: 'Cincinnati', country: 'United States' },
  { code: 'MCI', name: 'Kansas City International Airport', city: 'Kansas City', country: 'United States' },
  { code: 'OMA', name: 'Eppley Airfield', city: 'Omaha', country: 'United States' },
  { code: 'DSM', name: 'Des Moines International Airport', city: 'Des Moines', country: 'United States' },
  { code: 'RSW', name: 'Southwest Florida International Airport', city: 'Fort Myers', country: 'United States' },
  { code: 'JAX', name: 'Jacksonville International Airport', city: 'Jacksonville', country: 'United States' },
  { code: 'SAV', name: 'Savannah/Hilton Head International Airport', city: 'Savannah', country: 'United States' },
  { code: 'CHS', name: 'Charleston International Airport', city: 'Charleston', country: 'United States' },
  { code: 'MYR', name: 'Myrtle Beach International Airport', city: 'Myrtle Beach', country: 'United States' },
  
  // Major International Airports
  { code: 'LHR', name: 'Heathrow Airport', city: 'London', country: 'United Kingdom' },
  { code: 'LGW', name: 'Gatwick Airport', city: 'London', country: 'United Kingdom' },
  { code: 'STN', name: 'Stansted Airport', city: 'London', country: 'United Kingdom' },
  { code: 'LTN', name: 'Luton Airport', city: 'London', country: 'United Kingdom' },
  { code: 'CDG', name: 'Charles de Gaulle Airport', city: 'Paris', country: 'France' },
  { code: 'ORY', name: 'Orly Airport', city: 'Paris', country: 'France' },
  { code: 'FRA', name: 'Frankfurt am Main Airport', city: 'Frankfurt', country: 'Germany' },
  { code: 'MUC', name: 'Munich Airport', city: 'Munich', country: 'Germany' },
  { code: 'BER', name: 'Berlin Brandenburg Airport', city: 'Berlin', country: 'Germany' },
  { code: 'AMS', name: 'Amsterdam Airport Schiphol', city: 'Amsterdam', country: 'Netherlands' },
  { code: 'ZUR', name: 'Zurich Airport', city: 'Zurich', country: 'Switzerland' },
  { code: 'VIE', name: 'Vienna International Airport', city: 'Vienna', country: 'Austria' },
  { code: 'FCO', name: 'Leonardo da Vinci International Airport', city: 'Rome', country: 'Italy' },
  { code: 'MXP', name: 'Malpensa Airport', city: 'Milan', country: 'Italy' },
  { code: 'BCN', name: 'Barcelona-El Prat Airport', city: 'Barcelona', country: 'Spain' },
  { code: 'MAD', name: 'Adolfo Suárez Madrid-Barajas Airport', city: 'Madrid', country: 'Spain' },
  { code: 'LIS', name: 'Lisbon Airport', city: 'Lisbon', country: 'Portugal' },
  { code: 'ARN', name: 'Stockholm Arlanda Airport', city: 'Stockholm', country: 'Sweden' },
  { code: 'CPH', name: 'Copenhagen Airport', city: 'Copenhagen', country: 'Denmark' },
  { code: 'OSL', name: 'Oslo Airport', city: 'Oslo', country: 'Norway' },
  { code: 'HEL', name: 'Helsinki Airport', city: 'Helsinki', country: 'Finland' },
  { code: 'IST', name: 'Istanbul Airport', city: 'Istanbul', country: 'Turkey' },
  { code: 'DOH', name: 'Hamad International Airport', city: 'Doha', country: 'Qatar' },
  { code: 'DXB', name: 'Dubai International Airport', city: 'Dubai', country: 'United Arab Emirates' },
  { code: 'AUH', name: 'Abu Dhabi International Airport', city: 'Abu Dhabi', country: 'United Arab Emirates' },
  { code: 'CAI', name: 'Cairo International Airport', city: 'Cairo', country: 'Egypt' },
  { code: 'JNB', name: 'O.R. Tambo International Airport', city: 'Johannesburg', country: 'South Africa' },
  { code: 'CPT', name: 'Cape Town International Airport', city: 'Cape Town', country: 'South Africa' },
  
  // Major Asian Airports
  { code: 'NRT', name: 'Narita International Airport', city: 'Tokyo', country: 'Japan' },
  { code: 'HND', name: 'Haneda Airport', city: 'Tokyo', country: 'Japan' },
  { code: 'KIX', name: 'Kansai International Airport', city: 'Osaka', country: 'Japan' },
  { code: 'ICN', name: 'Incheon International Airport', city: 'Seoul', country: 'South Korea' },
  { code: 'GMP', name: 'Gimpo International Airport', city: 'Seoul', country: 'South Korea' },
  { code: 'PEK', name: 'Beijing Capital International Airport', city: 'Beijing', country: 'China' },
  { code: 'PKX', name: 'Beijing Daxing International Airport', city: 'Beijing', country: 'China' },
  { code: 'PVG', name: 'Shanghai Pudong International Airport', city: 'Shanghai', country: 'China' },
  { code: 'SHA', name: 'Shanghai Hongqiao International Airport', city: 'Shanghai', country: 'China' },
  { code: 'CAN', name: 'Guangzhou Baiyun International Airport', city: 'Guangzhou', country: 'China' },
  { code: 'HKG', name: 'Hong Kong International Airport', city: 'Hong Kong', country: 'Hong Kong' },
  { code: 'TPE', name: 'Taiwan Taoyuan International Airport', city: 'Taipei', country: 'Taiwan' },
  { code: 'SIN', name: 'Singapore Changi Airport', city: 'Singapore', country: 'Singapore' },
  { code: 'KUL', name: 'Kuala Lumpur International Airport', city: 'Kuala Lumpur', country: 'Malaysia' },
  { code: 'BKK', name: 'Suvarnabhumi Airport', city: 'Bangkok', country: 'Thailand' },
  { code: 'DMK', name: 'Don Mueang International Airport', city: 'Bangkok', country: 'Thailand' },
  { code: 'CGK', name: 'Soekarno-Hatta International Airport', city: 'Jakarta', country: 'Indonesia' },
  { code: 'MNL', name: 'Ninoy Aquino International Airport', city: 'Manila', country: 'Philippines' },
  { code: 'BOM', name: 'Chhatrapati Shivaji Maharaj International Airport', city: 'Mumbai', country: 'India' },
  { code: 'DEL', name: 'Indira Gandhi International Airport', city: 'Delhi', country: 'India' },
  { code: 'BLR', name: 'Kempegowda International Airport', city: 'Bangalore', country: 'India' },
  { code: 'HYD', name: 'Rajiv Gandhi International Airport', city: 'Hyderabad', country: 'India' },
  { code: 'MAA', name: 'Chennai International Airport', city: 'Chennai', country: 'India' },
  { code: 'CCU', name: 'Netaji Subhas Chandra Bose International Airport', city: 'Kolkata', country: 'India' },
  
  // Canadian Airports
  { code: 'YYZ', name: 'Toronto Pearson International Airport', city: 'Toronto', country: 'Canada' },
  { code: 'YUL', name: 'Montréal-Pierre Elliott Trudeau International Airport', city: 'Montreal', country: 'Canada' },
  { code: 'YVR', name: 'Vancouver International Airport', city: 'Vancouver', country: 'Canada' },
  { code: 'YYC', name: 'Calgary International Airport', city: 'Calgary', country: 'Canada' },
  { code: 'YEG', name: 'Edmonton International Airport', city: 'Edmonton', country: 'Canada' },
  { code: 'YOW', name: 'Ottawa Macdonald-Cartier International Airport', city: 'Ottawa', country: 'Canada' },
  { code: 'YHZ', name: 'Halifax Stanfield International Airport', city: 'Halifax', country: 'Canada' },
  { code: 'YWG', name: 'Winnipeg James Armstrong Richardson International Airport', city: 'Winnipeg', country: 'Canada' },
  
  // Australian/Oceania Airports
  { code: 'SYD', name: 'Sydney Kingsford Smith Airport', city: 'Sydney', country: 'Australia' },
  { code: 'MEL', name: 'Melbourne Airport', city: 'Melbourne', country: 'Australia' },
  { code: 'BNE', name: 'Brisbane Airport', city: 'Brisbane', country: 'Australia' },
  { code: 'PER', name: 'Perth Airport', city: 'Perth', country: 'Australia' },
  { code: 'ADL', name: 'Adelaide Airport', city: 'Adelaide', country: 'Australia' },
  { code: 'DRW', name: 'Darwin Airport', city: 'Darwin', country: 'Australia' },
  { code: 'AKL', name: 'Auckland Airport', city: 'Auckland', country: 'New Zealand' },
  { code: 'CHC', name: 'Christchurch Airport', city: 'Christchurch', country: 'New Zealand' },
  { code: 'WLG', name: 'Wellington Airport', city: 'Wellington', country: 'New Zealand' },
  
  // South American Airports
  { code: 'GRU', name: 'Sao Paulo-Guarulhos International Airport', city: 'Sao Paulo', country: 'Brazil' },
  { code: 'GIG', name: 'Rio de Janeiro-Galeão International Airport', city: 'Rio de Janeiro', country: 'Brazil' },
  { code: 'BSB', name: 'Brasília International Airport', city: 'Brasília', country: 'Brazil' },
  { code: 'EZE', name: 'Ezeiza International Airport', city: 'Buenos Aires', country: 'Argentina' },
  { code: 'SCL', name: 'Santiago International Airport', city: 'Santiago', country: 'Chile' },
  { code: 'LIM', name: 'Jorge Chávez International Airport', city: 'Lima', country: 'Peru' },
  { code: 'BOG', name: 'El Dorado International Airport', city: 'Bogotá', country: 'Colombia' },
  { code: 'CCS', name: 'Simón Bolívar International Airport', city: 'Caracas', country: 'Venezuela' },
  { code: 'UIO', name: 'Mariscal Sucre International Airport', city: 'Quito', country: 'Ecuador' },
];

export function searchAirports(query: string): Airport[] {
  if (!query || query.length < 1) {
    return [];
  }

  const searchTerm = query.toLowerCase();
  
  return airports.filter((airport) => {
    return (
      airport.code.toLowerCase().includes(searchTerm) ||
      airport.name.toLowerCase().includes(searchTerm) ||
      airport.city.toLowerCase().includes(searchTerm) ||
      airport.country.toLowerCase().includes(searchTerm)
    );
  }).slice(0, 10); // Limit to 10 results for performance
}
