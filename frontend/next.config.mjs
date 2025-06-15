/** @type {import('next').NextConfig} */
const nextConfig = {
    output: 'export',
    basePath: '/FlightTracker',  // Add this line
    images: {
        unoptimized: true,  // Required for static export
    },
};

export default nextConfig;