// user.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Participant {
  user_id: number;
  kills: number;
  username: string;
  email: string;
}

export interface LeaderboardEntry {
  leaderboard_id: string;
  waves_count: number;
  game_date: string;
  total_kills: string;
  participants: Participant[];
}

export interface LeaderboardResponse {
  message: string;
  leaderboard: LeaderboardEntry[];
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface RegisterResponse {
  message: string;
  access_token: string;
  refresh_token: string;
  type: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl = 'https://playsvlime.duckdns.org:4433/api/v1';

  constructor(private http: HttpClient) {}

  getLeaderboard(limit: number = 10, page: number = 1, numPlayers: number = 1): Observable<LeaderboardResponse> {
    const body = { limit, page, numPlayers };
    return this.http.post<LeaderboardResponse>(this.apiUrl + '/leaderboard/get', body);
  }

  register(data: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(this.apiUrl + '/auth/register', data);
  }
}
