type ColonReplacementFormat =
  | 'delete'
  | 'dash'
  | 'spaceDash'
  | 'spaceDashSpace'
  | 'smart';

export default interface NamingConfig {
  renameMovies: boolean;
  renameScenes: boolean;
  replaceIllegalCharacters: boolean;
  colonReplacementFormat: ColonReplacementFormat;
  standardMovieFormat: string;
  movieFolderFormat: string;
  standardSceneFormat: string;
  sceneFolderFormat: string;
  sceneImportFolderFormat: string;
  maxFolderPathLength: number;
  maxFilePathLength: number;
}
