﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using Breakfloor.UI;
using Breakfloor.Weapons;
using Breakfloor.HammerEnts;

namespace Breakfloor
{
	partial class BreakfloorGame : Game
	{
		public static readonly float StandardBlockSize = 64;
		public static readonly float StandardHalfBlockSize = StandardBlockSize / 2;
		public static readonly Vector3 StandardBlockDimensions = Vector3.One * StandardBlockSize;

		public static BreakfloorGame Instance => Current as BreakfloorGame;

		[Net]
		public RealTimeUntil RoundTimer { get; private set; } = 0f;

		private bool roundTimerStarted = false;

		public BreakfloorGame()
		{
			//
			// Create the HUD entity. This is always broadcast to all clients
			// and will create the UI panels clientside. It's accessible 
			// globally via Hud.Current, so we don't need to store it.
			//
			if ( IsServer )
			{
				// This is really silly, I know. I assume we'll be able to store 
				// at the very least a JSON/.txt file or something on the s&works addon page for secrets some day.
				Admins = new List<long>() { 76561197998255119 };
				
				_ = new BreakfloorHud();
				RoundTimer = RoundTimeCvar; //so the timer can be frozen at the roundtimecvar.
			}

		}

		public override void ClientJoined( Client cl )
		{
			var isAdmin = Admins.Contains( cl.PlayerId );
			Log.Info( $"\"{cl.Name}\" has joined the game" );
			BFChatbox.AddInformation( To.Everyone, $"{cl.Name} has joined", $"avatar:{cl.PlayerId}", isAdmin );

			//Decide which team the client will be on.
			var teamDifference = TeamA.Count - TeamB.Count;
			if ( teamDifference < 0 )
			{
				JoinTeam( cl, Teams.A );
			}
			else if ( teamDifference > 0 )
			{
				JoinTeam( cl, Teams.B );
			}
			else
			{
				JoinRandomTeam( cl );
			}

			var player = new BreakfloorPlayer();
			cl.Pawn = player;
			player.UpdateClothes( cl );
			player.Respawn();

			//BFChatbox.AddInformation( To.Single( cl ),
			//	$"Welcome to Breakfloor! You can toggle auto reloading by typing \"bf_auto_reload true\" into the console." );

			//Update the status of the round timer AFTER the joining client's
			//team is set.
			if ( !roundTimerStarted && (TeamA.Count >= 1 && TeamB.Count >= 1) )
			{
				RestartRound();
				RoundTimer = RoundTimeCvar;
				roundTimerStarted = true;
				Log.Info( "Starting round!" );
			}
		}

		public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
		{
			if ( TeamA.Contains( cl ) ) TeamA.Remove( cl );
			if ( TeamB.Contains( cl ) ) TeamB.Remove( cl );
			base.ClientDisconnect( cl, reason );
		}

		[Event.Tick.Server]
		public void ServerTick()
		{
			if ( roundTimerStarted )
			{
				if ( RoundTimer <= 0 )
				{
					//handle restart round and restart the timer, etc.
					RestartRound();
				}
			}
			else
			{
				RoundTimer = RoundTimeCvar + 1;
			}
		}

		[ConCmd.Admin( "bf_restart" )]
		public static void BfRestart()
		{
			(Game.Current as BreakfloorGame).RestartRound();
		}

		public void RestartRound()
		{
			//gotta love foreachs :D

			foreach ( var block in Entity.All.OfType<BreakFloorBlock>() )
			{
				block.Reset();
			}

			foreach ( var e in WorldEntity.All )
			{
				e.RemoveAllDecals();
			}

			foreach ( var c in Client.All )
			{
				if ( c.Pawn is BreakfloorPlayer ply )
				{
					ply.Respawn();
					c.SetInt( "kills", 0 );
					c.SetInt( "deaths", 0 );
				}

			}

			RoundTimer = RoundTimeCvar;
		}

		public override void OnKilled( Client victimClient, Entity victimPawn )
		{
			//override the base.onkilled and tweak it instead of calling base. it does shit we dont want.
			Host.AssertServer();

			var vic = (BreakfloorPlayer)victimPawn;

			if ( victimPawn.LastAttacker == null ) return;

			
			if ( victimPawn.LastAttacker.GetType() == typeof( HurtVolumeEntity ) ) // First check if we died to a map hurt trigger
			{
				var block = vic.LastBlockStoodOn;
				if ( block != null && block.Broken )
				{
					if ( block.LastAttacker == vic ) //Player caused their own downfall
					{
						var suicideText = new string[3] { "got themself killed", "played themself", "dug straight down" };
						OnKilledClient( To.Everyone,
							victimClient,
							null,
							Rand.FromArray<string>( suicideText ) );
					}
					else //Other player caused it
					{
						OnKilledClient( To.Everyone,
							block.LastAttacker.Client,
							victimClient,
							"BREAKFLOORED" );

						block.LastAttacker.Client.AddInt( "kills" );
					}
					return;
				}

				//Otherwise, they just fell through a hole in the blocks and died
				OnKilledClient( To.Everyone,
					victimClient,
					null,
					"died" );
				return;
			}

			// Player didn't die from falling or a hurt trigger then.
			// They died to a gun, how boring.
			if ( victimPawn.LastAttacker.Client != null )
			{
				var killedByText = (victimPawn.LastAttackerWeapon as BreakfloorWeapon).GetKilledByText();

				if ( string.IsNullOrEmpty( killedByText ) )
				{
					killedByText = victimPawn.LastAttackerWeapon.ClassName;
				}

				OnKilledClient( To.Everyone, victimPawn.LastAttacker.Client, victimClient, killedByText );
			}

		}

		[ClientRpc]
		public void OnKilledClient( Client killer, Client victim, string method )
		{
			KillFeed.Current.AddEntry( killer, victim, method );
		}
	}
}

