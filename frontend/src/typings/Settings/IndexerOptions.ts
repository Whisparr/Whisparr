export default interface IndexerOptions {
  minimumAge: number;
  retention: number;
  searchStudioCode: boolean;
  searchStudioDate: boolean;
  searchStudioFormat: string;
  searchStudioTitle: boolean;
  searchDateFormat: string;
  searchTitleDate: boolean;
  searchTitleOnly: boolean;
  maximumSize: number;
  rssSyncInterval: number;
  preferIndexerFlags: boolean;
  availabilityDelay: number;
  whitelistedHardcodedSubs: string[];
  allowHardcodedSubs: boolean;
}
