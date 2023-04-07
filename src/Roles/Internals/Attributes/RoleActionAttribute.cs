using System;
using System.Collections.Generic;

// ReSharper disable InvalidXmlDocComment

namespace TOHTOR.Roles.Internals.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class RoleActionAttribute: Attribute
{
    public RoleActionType ActionType { get; }
    public bool WorksAfterDeath { get; }
    public Priority Priority { get; }
    public bool Blockable { set; get; }
    /// <summary>
    /// If provided, overrides any methods of the same action with the same name from any parent classes
    /// </summary>
    public String? Override;
    /// <summary>
    /// Dictates whether this action should be utilized in subclasses of the class declaring this method <b>Default: True</b>
    /// </summary>
    public bool Subclassing = true;

    public RoleActionAttribute(RoleActionType actionType, bool triggerAfterDeath = false, bool blockable = true, Priority priority = Priority.NoPriority)
    {
        this.ActionType = actionType;
        this.WorksAfterDeath = triggerAfterDeath || actionType is RoleActionType.MyDeath;
        this.Priority = priority;
        this.Blockable = blockable && actionType is not RoleActionType.MyVote;
    }

    public override string ToString() => $"RoleAction(type={ActionType}, Priority={Priority}, Blockable={Blockable}, Subclassing={Subclassing}, Override={Override})";
}

public enum Priority
{
    First,
    NoPriority,
    Last
}

public enum RoleActionType
{
    /// <summary>
    /// Represents no action
    /// </summary>
    None,
    /// <summary>
    /// Any action specifically taken by a player
    /// Parameters: (PlayerControl source, RoleAction action, object[] parameters)
    /// </summary>
    AnyPlayerAction,
    OnPet,
    /// <summary>
    /// Triggers whenever the player enters a vent (this INCLUDES vent activation)
    /// Parameters: (Vent vent)
    /// </summary>
    MyEnterVent,
    /// <summary>
    /// Triggered when a player ACTUALLY enters a vent (not just Vent activation)
    /// Parameters: (Vent vent, PlayerControl venter)
    /// </summary>
    AnyEnterVent,
    VentExit,
    SuccessfulAngelProtect,
    SabotageStarted,
    /// <summary>
    /// Triggered when any one player fixes any part of a sabotage (I.E MiraHQ Comms) <br></br>
    /// Parameters: (SabotageType type, PlayerControl fixer, byte fixBit)
    /// </summary>
    SabotagePartialFix,
    SabotageFixed,
    Shapeshift,
    Unshapeshift,
    /// <summary>
    /// Triggered when my player attacks another player<br/>
    /// Parameters: (PlayerControl target)
    /// </summary>
    Attack,
    /// <summary>
    /// Triggered when my player dies. This action <b>CANNOT</b> be canceled. <br/>
    /// <b>Parameters -</b> (PlayerControl killer)
    /// </summary>
    MyDeath,
    SelfExiled,
    /// <summary>
    /// Triggers when any player gets exiled (by being voted out)
    /// </summary>
    /// <param name="exiled"><see cref="GameData.PlayerInfo"/> the exiled player</param>
    AnyExiled,
    /// <summary>
    /// Triggers on Round Start (end of meetings, and start of game)
    /// Parameters: (bool isRoundOne)
    /// </summary>
    RoundStart,
    RoundEnd,
    SelfReportBody,
    /// <summary>
    /// Triggers when any player reports a body. <br></br>Parameters: (PlayerControl reporter, PlayerInfo reported)
    /// </summary>
    AnyReportedBody,
    TaskComplete,
    FixedUpdate,
    /// <summary>
    /// Triggers when any player dies. This cannot be canceled
    /// </summary>
    /// <param name="victim"><see cref="PlayerControl"/> the dead player</param>
    /// <param name="killer"><see cref="PlayerControl"/> the killing player</param>
    AnyDeath,
    /// <summary>
    /// Triggers when my player votes for someone (or skips)
    /// </summary>
    /// <param name="voted"><see cref="PlayerControl"/> the player voted for, or null if skipped</param>
    /// <param name="delegate"><see cref="MeetingDelegate"/> the meeting delegate for the current meeting</param>
    MyVote,
    /// <summary>
    /// Triggers when any player votes for someone (or skips)
    /// </summary>
    /// <param name="voter"><see cref="PlayerControl"/> the player voting</param>
    /// <param name="voted"><see cref="PlayerControl"/> the player voted for, or null if skipped</param>
    /// <param name="delegate"><see cref="MeetingDelegate"/> the meeting delegate for the current meeting</param>
    AnyVote,
    /// <summary>
    /// Triggers whenever another player interacts with THIS role
    /// </summary>
    /// <param name="interactor"><see cref="PlayerControl"/> the player starting the interaction</param>
    /// <param name="interaction"><see cref="Interaction"/> the interaction</param>
    Interaction,
    /// <summary>
    /// Triggers whenever another player interacts with any other player
    /// </summary>
    /// <param name="interactor"><see cref="PlayerControl"/> the player starting the interaction</param>
    /// <param name="target"><see cref="PlayerControl"/> the player being interacted with</param>
    /// <param name="interaction"><see cref="Interaction"/> the interaction</param>
    AnyInteraction,
    /// <summary>
    /// Triggers whenever a player sends a chat message. This action cannot be canceled.
    /// </summary>
    /// <param name="sender"><see cref="PlayerControl"/> the player who sent the chat message</param>
    /// <param name="message"><see cref="string"/> the message sent</param>
    /// <param name="state"><see cref="TOHTOR.API.GameState"/> the current state of the game (for checking in meeting)</param>
    /// <param name="isAlive"><see cref="bool"/> if the chatting player is alive</param>
    Chat
}

public static class RoleActionTypeMethods
{
    // ReSharper disable once CollectionNeverUpdated.Global
    public static readonly HashSet<RoleActionType> PlayerActions = new();

    public static bool IsPlayerAction(this RoleActionType actionType)
    {
        return (actionType) switch
        {
            RoleActionType.None => false,
            RoleActionType.AnyPlayerAction => false,
            RoleActionType.OnPet => true,
            RoleActionType.MyEnterVent => true,
            RoleActionType.AnyEnterVent => true,
            RoleActionType.VentExit => true,
            RoleActionType.SuccessfulAngelProtect => false,
            RoleActionType.SabotageStarted => true,
            RoleActionType.SabotagePartialFix => true,
            RoleActionType.SabotageFixed => true,
            RoleActionType.Shapeshift => true,
            RoleActionType.Unshapeshift => true,
            RoleActionType.Attack => true,
            RoleActionType.MyDeath => false,
            RoleActionType.SelfExiled => false,
            RoleActionType.AnyExiled => false,
            RoleActionType.RoundStart => false,
            RoleActionType.RoundEnd => false,
            RoleActionType.SelfReportBody => true,
            RoleActionType.AnyReportedBody => false,
            RoleActionType.TaskComplete => false,
            RoleActionType.FixedUpdate => false,
            RoleActionType.AnyDeath => false,
            RoleActionType.MyVote => true,
            RoleActionType.AnyVote => false,
            RoleActionType.Interaction => false,
            RoleActionType.AnyInteraction => false,
            _ => PlayerActions.Contains(actionType)
        };
    }
}