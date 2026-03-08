# Displayed as initiator of vote when no user creates the vote
ui-vote-initiator-server = The server

## Default.Votes

ui-vote-restart-title = Restart round
ui-vote-restart-succeeded = Restart vote succeeded.
ui-vote-restart-failed = Restart vote failed (need { TOSTRING($ratio, "P0") }).
ui-vote-restart-fail-not-enough-ghost-players = Restart vote failed: A minimum of { $ghostPlayerRequirement }% ghost players is required to initiate a restart vote. Currently, there are not enough ghost players.
# #Misfits Change: Majority-based failure message referencing total connected player count.
ui-vote-restart-failed-majority = Restart vote failed: { $yes }/{ $total } voted yes (need { $needed } for majority).
ui-vote-restart-yes = Yes
ui-vote-restart-no = No
ui-vote-restart-abstain = Abstain

# #Misfits Change: Vote to extend the round.
ui-vote-extend-title = Extend round
# Misfits Change - wasteland theme: shuttle → train
#ui-vote-extend-succeeded = Extend vote passed — round extended by { $minutes } minutes. Shuttle recalled.
ui-vote-extend-succeeded = Extend vote passed — round extended by { $minutes } minutes. Train recalled.
ui-vote-extend-failed = Extend vote failed: { $yes }/{ $total } voted yes (need { $needed } for majority).
ui-vote-extend-yes = Yes
ui-vote-extend-no = No
ui-vote-extend-abstain = Abstain

# #Misfits Change: Round countdown announcements.
ui-round-countdown-60 = Attention: Approximately 60 minutes remain before the round ends.
ui-round-countdown-30 = Attention: Approximately 30 minutes remain before the round ends.
ui-round-countdown-15 = Attention: Approximately 15 minutes remain. A vote to decide the round's fate will now begin.

# #Misfits Change: Automatic round-decision vote (New Round vs Extend Round).
ui-vote-round-decision-title = Round ending soon — what should happen?
ui-vote-round-decision-new = New Round
ui-vote-round-decision-extend = Extend Round
ui-vote-round-decision-new-won = The vote has decided: New Round ({ $newVotes } vs { $extendVotes }, { $total } connected). Calling the shuttle.
ui-vote-round-decision-extend-won = The vote has decided: Extend Round ({ $extendVotes } vs { $newVotes }, { $total } connected). The round continues!
ui-vote-round-decision-tie = The vote was tied ({ $votes } each, { $total } connected). Defaulting to extending the round.

ui-vote-gamemode-title = Next gamemode
ui-vote-gamemode-tie = Tie for gamemode vote! Picking... { $picked }
ui-vote-gamemode-win = { $winner } won the gamemode vote!

ui-vote-map-title = Next map
ui-vote-map-tie = Tie for map vote! Picking... { $picked }
ui-vote-map-win = { $winner } won the map vote!
ui-vote-map-notlobby = Voting for maps is only valid in the pre-round lobby!
ui-vote-map-notlobby-time = Voting for maps is only valid in the pre-round lobby with { $time } remaining!
