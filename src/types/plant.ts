export interface PlantSummary {
  id: number;
  name: string;
  species: string | null;
  variety: string | null;
  location: string | null;
  acquiredDate: string | null;
  notes: string | null;
  coverImageUrl: string | null;
  createdAtUtc: string;
  lastObservedAtUtc: string | null;
  lastWateredAtUtc: string | null;
}

export interface Plant extends Omit<PlantSummary, "lastObservedAtUtc" | "lastWateredAtUtc"> {}

export interface PlantObservation {
  id: number;
  observedAtUtc: string;
  notes: string | null;
  imageUrl: string | null;
  issueSummary: string | null;
}

export interface PlantCareEvent {
  id: number;
  eventType: "Watered" | "Fed" | "Pruned" | "Repotted" | "Treated" | "Moved" | "Other" | string;
  occurredAtUtc: string;
  notes: string | null;
}

export interface PlantDetailResponse {
  plant: Plant;
  observations: PlantObservation[];
  careEvents: PlantCareEvent[];
}

export interface PlantPayload {
  name: string;
  species: string | null;
  variety: string | null;
  location: string | null;
  acquiredDate: string | null;
  notes: string | null;
}

export interface PlantIdentification {
  commonName: string | null;
  scientificName: string | null;
  variety: string | null;
  confidence: number;
  alternatives: string[];
  notes: string;
}

export interface PlantSituation {
  setting: "outdoor" | "indoor" | "unclear";
  growingIn: "pot" | "container" | "ground" | "unclear";
  apparentLight: string;
  soilSurface: string;
  growthStage: string;
  visibleCondition: string;
}

export interface PlantAnalysis {
  identification: PlantIdentification;
  situation: PlantSituation;
  status: "healthy" | "watch" | "issue";
  summary: string;
  possibleIssues: string[];
  suggestedActions: string[];
  urgent: boolean;
}


export interface GardenWeatherDay {
  date: string;
  temperatureMaxC: number;
  temperatureMinC: number;
  rainMm: number;
  rainProbabilityPercent: number;
  et0Mm: number;
  windMaxKmh: number;
}

export interface ClimateBaseline {
  startYear: number;
  endYear: number;
  month: number;
  averageMaxTemperatureC: number;
  averageMinTemperatureC: number;
  averageDailyRainMm: number;
  averageDailyEt0Mm: number;
}

export interface GardenWeatherContext {
  location: string;
  baselineYears: number;
  today: GardenWeatherDay;
  rainPast3DaysMm: number;
  rainNext48HoursMm: number;
  rainNext7DaysMm: number;
  currentMonthBaseline: ClimateBaseline;
  forecast: GardenWeatherDay[];
}

export interface PlantCareAdvice {
  action: "skip" | "check" | "monitor" | "water" | string;
  title: string;
  summary: string;
  routineGuidance: string;
  climateComparison: string;
  reasons: string[];
  daysSinceWatered: number | null;
  weather: GardenWeatherContext;
}
