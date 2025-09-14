'use client';

import { FlightOption } from '@/lib/schemas';
import { 
  WifiIcon, 
  TvIcon, 
  BoltIcon, 
  CakeIcon,
  MapPinIcon,
  PaperAirplaneIcon
} from '@heroicons/react/24/outline';
import { formatPrice } from '@/lib/api';
import { format } from 'date-fns';

interface FlightDetailsProps {
  flight: FlightOption;
}

export function FlightDetails({ flight }: FlightDetailsProps) {
  const formatTime = (timeString: string) => {
    return format(new Date(timeString), 'HH:mm');
  };

  const formatDate = (timeString: string) => {
    return format(new Date(timeString), 'MMM d');
  };

  // Mock data for demonstration - in real app this would come from API
  const mockDetails = {
    departureTerminal: 'Terminal 1',
    departureGate: 'A12',
    arrivalTerminal: 'Terminal 3',
    arrivalGate: 'B8',
    aircraftModel: 'Boeing 777-300ER',
    registration: 'PH-BVA',
    amenities: {
      wifi: true,
      entertainment: true,
      power: true,
      meals: 'Complimentary meal service',
      legroom: '79 cm (31 inches)'
    },
    fareBreakdown: {
      baseFare: Math.round(flight.price.amount * 0.75),
      taxes: Math.round(flight.price.amount * 0.20),
      fees: Math.round(flight.price.amount * 0.05),
      currency: flight.price.currency
    },
    policies: {
      cancellation: 'Free cancellation within 24 hours',
      changes: 'Changes allowed with fee',
      baggage: '1 checked bag (23kg) included'
    }
  };

  return (
    <div className="border-t border-neutral-200 bg-neutral-50 p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center">
        <h3 className="text-lg font-semibold text-neutral-900">Flight Details</h3>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Flight Route Details */}
        <div className="bg-white rounded-lg p-4">
          <h4 className="font-medium text-neutral-900 mb-3 flex items-center gap-2">
            <MapPinIcon className="w-5 h-5" />
            Route Information
          </h4>
          <div className="space-y-4">
            {/* Departure */}
            <div>
              <div className="text-sm text-neutral-500">Departure</div>
              <div className="font-medium">{flight.origin.code} - {flight.origin.name}</div>
              <div className="text-sm text-neutral-600">
                {formatTime(flight.departureTime)} on {formatDate(flight.departureTime)}
              </div>
              {mockDetails.departureTerminal && (
                <div className="text-sm text-neutral-600">
                  {mockDetails.departureTerminal} • Gate {mockDetails.departureGate}
                </div>
              )}
            </div>
            
            {/* Arrival */}
            <div>
              <div className="text-sm text-neutral-500">Arrival</div>
              <div className="font-medium">{flight.destination.code} - {flight.destination.name}</div>
              <div className="text-sm text-neutral-600">
                {formatTime(flight.arrivalTime)} on {formatDate(flight.arrivalTime)}
              </div>
              {mockDetails.arrivalTerminal && (
                <div className="text-sm text-neutral-600">
                  {mockDetails.arrivalTerminal} • Gate {mockDetails.arrivalGate}
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Aircraft & Amenities */}
        <div className="bg-white rounded-lg p-4">
          <h4 className="font-medium text-neutral-900 mb-3 flex items-center gap-2">
            <PaperAirplaneIcon className="w-5 h-5" />
            Aircraft & Amenities
          </h4>
          <div className="space-y-3">
            {/* Aircraft Info */}
            <div>
              <div className="text-sm text-neutral-500">Aircraft</div>
              <div className="font-medium">{mockDetails.aircraftModel}</div>
              <div className="text-sm text-neutral-600">Registration: {mockDetails.registration}</div>
            </div>
            
            {/* Amenities */}
            <div>
              <div className="text-sm text-neutral-500 mb-2">Amenities</div>
              <div className="space-y-1">
                <div className="flex items-center gap-2 text-sm">
                  <WifiIcon className={`w-4 h-4 ${mockDetails.amenities.wifi ? 'text-green-500' : 'text-neutral-300'}`} />
                  <span className={mockDetails.amenities.wifi ? 'text-neutral-700' : 'text-neutral-400'}>
                    Wi-Fi {mockDetails.amenities.wifi ? 'Available' : 'Not Available'}
                  </span>
                </div>
                <div className="flex items-center gap-2 text-sm">
                  <TvIcon className={`w-4 h-4 ${mockDetails.amenities.entertainment ? 'text-green-500' : 'text-neutral-300'}`} />
                  <span className={mockDetails.amenities.entertainment ? 'text-neutral-700' : 'text-neutral-400'}>
                    Entertainment {mockDetails.amenities.entertainment ? 'Available' : 'Not Available'}
                  </span>
                </div>
                <div className="flex items-center gap-2 text-sm">
                  <BoltIcon className={`w-4 h-4 ${mockDetails.amenities.power ? 'text-green-500' : 'text-neutral-300'}`} />
                  <span className={mockDetails.amenities.power ? 'text-neutral-700' : 'text-neutral-400'}>
                    Power Outlets {mockDetails.amenities.power ? 'Available' : 'Not Available'}
                  </span>
                </div>
                <div className="flex items-center gap-2 text-sm">
                  <CakeIcon className="w-4 h-4 text-green-500" />
                  <span className="text-neutral-700">{mockDetails.amenities.meals}</span>
                </div>
              </div>
            </div>

            {/* Legroom */}
            <div>
              <div className="text-sm text-neutral-500">Legroom</div>
              <div className="text-sm text-neutral-700">{mockDetails.amenities.legroom}</div>
            </div>
          </div>
        </div>

        {/* Fare & Policies */}
        <div className="bg-white rounded-lg p-4">
          <h4 className="font-medium text-neutral-900 mb-3">Fare & Policies</h4>
          <div className="space-y-4">
            {/* Fare Breakdown */}
            <div>
              <div className="text-sm text-neutral-500 mb-2">Fare Breakdown</div>
              <div className="space-y-1 text-sm">
                <div className="flex justify-between">
                  <span className="text-neutral-600">Base Fare</span>
                  <span className="text-neutral-900">
                    {formatPrice(mockDetails.fareBreakdown.baseFare, mockDetails.fareBreakdown.currency)}
                  </span>
                </div>
                <div className="flex justify-between">
                  <span className="text-neutral-600">Taxes & Fees</span>
                  <span className="text-neutral-900">
                    {formatPrice(mockDetails.fareBreakdown.taxes + mockDetails.fareBreakdown.fees, mockDetails.fareBreakdown.currency)}
                  </span>
                </div>
                <div className="border-t border-neutral-200 pt-1">
                  <div className="flex justify-between font-medium">
                    <span>Total</span>
                    <span>{formatPrice(flight.price.amount, flight.price.currency)}</span>
                  </div>
                </div>
              </div>
            </div>

            {/* Policies */}
            <div>
              <div className="text-sm text-neutral-500 mb-2">Policies</div>
              <div className="space-y-2 text-sm text-neutral-700">
                <div>
                  <span className="font-medium">Cancellation:</span> {mockDetails.policies.cancellation}
                </div>
                <div>
                  <span className="font-medium">Changes:</span> {mockDetails.policies.changes}
                </div>
                <div>
                  <span className="font-medium">Baggage:</span> {mockDetails.policies.baggage}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Segments for connecting flights */}
      {flight.stops > 0 && (
        <div className="bg-white rounded-lg p-4">
          <h4 className="font-medium text-neutral-900 mb-3">Flight Segments</h4>
          <div className="text-sm text-neutral-600">
            This flight has {flight.stops} stop{flight.stops > 1 ? 's' : ''}. Detailed segment information would be displayed here.
          </div>
        </div>
      )}
    </div>
  );
}
