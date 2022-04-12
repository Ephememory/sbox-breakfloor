﻿using Sandbox.UI.Construct;
using Sandbox;
using Sandbox.UI;
using System;

namespace Breakfloor.UI
{
	public partial class BFChatbox : Panel
	{
		static BFChatbox Current;

		public Panel Canvas { get; protected set; }
		public TextEntry Input { get; protected set; }

		public BFChatbox()
		{
			Current = this;

			StyleSheet.Load( "/ui/Chat/BFChatBox.scss" );

			Canvas = Add.Panel( "chat_canvas" );

			Input = Add.TextEntry( "" );
			Input.AddEventListener( "onsubmit", () => Submit() );
			Input.AddEventListener( "onblur", () => Close() );
			Input.AcceptsFocus = true;
			Input.AllowEmojiReplace = true;

			Sandbox.Hooks.Chat.OnOpenChat += Open;
		}

		void Open()
		{
			AddClass( "open" );
			Input.Focus();
		}

		void Close()
		{
			RemoveClass( "open" );
			Input.Blur();
		}

		void Submit()
		{
			Close();

			var msg = Input.Text.Trim();
			Input.Text = "";

			if ( string.IsNullOrWhiteSpace( msg ) )
				return;

			Say( msg );
		}

		public void AddEntry( string name, string message, string avatar, string lobbyState = null, bool admin = false )
		{
			BFChatEntry e = Canvas.AddChild<BFChatEntry>();

			e.Message.Text = message;
			e.NameLabel.Text = string.Concat( name, " :" );
			e.Avatar.SetTexture( avatar );

			e.SetClass( "noname", string.IsNullOrEmpty( name ) );
			e.SetClass( "noavatar", string.IsNullOrEmpty( avatar ) );
			e.SetClass( "is-dev", admin );

			if ( lobbyState == "ready" || lobbyState == "staging" )
			{
				e.SetClass( "is-lobby", true );
			}


		}

		[ClientCmd( "chat_add", CanBeCalledFromServer = true )]
		public static void AddChatEntry( string name, string message, string avatar = null, string lobbyState = null, bool isAdmin = false )
		{

			Current?.AddEntry( name, message, avatar, lobbyState, admin: isAdmin );

			// Only log clientside if we're not the listen server host
			if ( !Global.IsListenServer )
			{
				Log.Info( $"{name}: {message}" );
			}
		}

		[ClientCmd( "chat_addinfo", CanBeCalledFromServer = true )]
		public static void AddInformation( string message, string avatar = null, bool isPlayerAdmin = false )
		{
			Current?.AddEntry( null, message, avatar, admin: isPlayerAdmin );
		}

		[ServerCmd( "say" )]
		public static void Say( string message )
		{
			Assert.NotNull( ConsoleSystem.Caller );

			if ( ConsoleSystem.Caller.GetValue<bool>( "gagged" ) ) return;

			// todo - reject more stuff
			if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
				return;

			var admin = Breakfloor.BreakfloorGame.Devs.Contains( ConsoleSystem.Caller.PlayerId );
			Log.Info( $"{ConsoleSystem.Caller}: {message}" );
			AddChatEntry( To.Everyone, ConsoleSystem.Caller.Name, message, $"avatar:{ConsoleSystem.Caller.PlayerId}", isAdmin: admin );
		}
	}
}