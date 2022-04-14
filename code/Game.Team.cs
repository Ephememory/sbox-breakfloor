﻿using Breakfloor.UI;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Breakfloor
{
	//it makes more sense to use an enum but because of the way clientdata works its just better to use static ints lol
	//public enum Team : int
	//{
	//	None = -1,
	//	A = 0,
	//	B = 1
	//}

	public partial class BreakfloorGame : Game
	{
		public const int TeamIndexNone = -1;
		public const int TeamIndexA = 0;
		public const int TeamIndexB = 1;

		public static List<Client> TeamA = new List<Client>();
		public static List<Client> TeamB = new List<Client>();

		public static int GetMyTeam( Client client )
		{
			return TeamA.Contains( client ) ? TeamIndexA : TeamIndexB;
		}

		public static Color GetTeamColor( int index )
		{
			switch ( index )
			{
				case TeamIndexA:
					return Color.FromBytes( 237, 36, 79 );
				case TeamIndexB:
					return Color.FromBytes( 26, 154, 240 );
				default:
					return Color.White;
			}
		}

		public void JoinTeam( Client p, int index )
		{
			switch ( index )
			{
				case TeamIndexA:
					if ( !TeamA.Contains( p.Client ) )
					{
						TeamA.Add( p );
						p.SetValue( "team", TeamIndexA );
						Log.Info( $"Client:{p} joined team A." );
						BFChatbox.AddInformation( To.Everyone, $"{p.Name} joined team RED.", $"avatar:{p.PlayerId}", isPlayerAdmin: false );
					}
					break;
				case TeamIndexB:
					if ( !TeamB.Contains( p.Client ) )
					{
						TeamB.Add( p );
						p.SetValue( "team", TeamIndexB );
						Log.Info( $"Client:{p} joined team B." );
						BFChatbox.AddInformation( To.Everyone, $"{p.Name} joined team BLUE.", $"avatar:{p.PlayerId}", isPlayerAdmin: false );
					}
					break;
			}
		}

		public void JoinRandomTeam( Client client )
		{
			Log.Info( $"Joining random team:{client}" );

			var randomValue = Rand.Int( 0, 2 );
			if ( randomValue == 1 )
			{
				JoinTeam( client, TeamIndexA );
			}
			else
			{
				JoinTeam( client, TeamIndexB );
			}
		}

		public static bool SameTeam( Client a, Client b )
		{
			if ( a == null || b == null ) return false;

			int aValue = a.GetValue<int>( "team" );
			int bValue = b.GetValue<int>( "team" );
			return aValue == bValue;
		}
	}
}
