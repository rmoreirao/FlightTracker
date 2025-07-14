/** @type {import('next').NextConfig} */
const nextConfig = {
    output: 'export',
    // basePath: '',  // Add this line
    images: {
        unoptimized: true,  // Required for static export
    },
};

export default nextConfig;