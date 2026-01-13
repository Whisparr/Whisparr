import ModelBase from 'App/ModelBase';

export default interface ImportListExclusion extends ModelBase {
  foreignId: string;
  movieTitle: string;
  movieYear: number | null;
  type: 'movie' | 'scene' | 'studio' | 'performer' | 'tag';
}
