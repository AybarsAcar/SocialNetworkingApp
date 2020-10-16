import { Photo } from './photo';

export interface Member {
  id: number;
  userName: string;
  knownAs: string;
  created: Date;
  lastActive: Date;
  gender: string;
  introduction: string;
  lookingFor: string;
  interests: string;
  city: string;
  country: string;
  age: number;
  photoUrl: string;
  photos: Photo[];
}
