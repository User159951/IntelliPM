export default function GeometricShapes() {
  return (
    <div className="absolute inset-0 overflow-hidden">
      {/* Large hexagon */}
      <div className="absolute top-20 left-10 animate-float">
        <svg width="120" height="104" viewBox="0 0 120 104" fill="none" className="opacity-20">
          <path
            d="M60 0L113.923 31V73L60 104L6.07697 73V31L60 0Z"
            fill="currentColor"
            className="text-white"
          />
        </svg>
      </div>

      {/* Small hexagon */}
      <div className="absolute top-40 right-20 animate-float-delayed">
        <svg width="60" height="52" viewBox="0 0 120 104" fill="none" className="opacity-30">
          <path
            d="M60 0L113.923 31V73L60 104L6.07697 73V31L60 0Z"
            stroke="currentColor"
            strokeWidth="3"
            className="text-white"
          />
        </svg>
      </div>

      {/* Circle with gradient */}
      <div className="absolute bottom-32 left-1/4 animate-pulse-glow">
        <div className="w-32 h-32 rounded-full bg-gradient-to-br from-white/20 to-transparent" />
      </div>

      {/* Dotted circle */}
      <div className="absolute top-1/3 right-10 animate-float">
        <svg width="80" height="80" viewBox="0 0 80 80" fill="none" className="opacity-25">
          <circle
            cx="40"
            cy="40"
            r="38"
            stroke="currentColor"
            strokeWidth="2"
            strokeDasharray="6 6"
            className="text-white"
          />
        </svg>
      </div>

      {/* Triangle */}
      <div className="absolute bottom-20 right-1/4 animate-float-delayed">
        <svg width="80" height="70" viewBox="0 0 80 70" fill="none" className="opacity-20">
          <path
            d="M40 0L80 70H0L40 0Z"
            fill="currentColor"
            className="text-white"
          />
        </svg>
      </div>

      {/* Small circles */}
      <div className="absolute top-1/2 left-20 animate-pulse-glow" style={{ animationDelay: '1s' }}>
        <div className="w-4 h-4 rounded-full bg-white/40" />
      </div>
      <div className="absolute top-1/4 left-1/3 animate-pulse-glow" style={{ animationDelay: '2s' }}>
        <div className="w-3 h-3 rounded-full bg-white/30" />
      </div>
      <div className="absolute bottom-1/3 left-16 animate-pulse-glow" style={{ animationDelay: '0.5s' }}>
        <div className="w-2 h-2 rounded-full bg-white/50" />
      </div>

      {/* Lines */}
      <div className="absolute top-0 left-0 w-full h-full opacity-10">
        <svg width="100%" height="100%" className="absolute">
          <line x1="0" y1="30%" x2="60%" y2="100%" stroke="white" strokeWidth="1" />
          <line x1="20%" y1="0" x2="80%" y2="70%" stroke="white" strokeWidth="1" />
        </svg>
      </div>

      {/* Grid pattern overlay */}
      <div 
        className="absolute inset-0 opacity-5"
        style={{
          backgroundImage: `
            linear-gradient(rgba(255,255,255,0.1) 1px, transparent 1px),
            linear-gradient(90deg, rgba(255,255,255,0.1) 1px, transparent 1px)
          `,
          backgroundSize: '40px 40px'
        }}
      />
    </div>
  );
}
