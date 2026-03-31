$MarbleNPC::Quotes = -1;
datablock MarbleData(MarbleNPC : DefaultMarble) {
	accelerationSpeed = 2;
	targetSpeed = 5;
	maxRollSpeed = 30;
	randomFactor = 5;
	updatePeriod = 10; //TODO changing this breaks stuff?
	scoutPeriod = 500;
	jumpPeriod = 100;
	maxStrayDistance = 20;
	teleportDistance = 50;
	idleTextWaitTime = 5000;
	idleTextPeriodMin = 3000;
	idleTextPeriodMax = 10000;
	idleTextDuration = 3000;
	respawnDelay = 2000;
	//mass = 3.375;
	
	randText[$MarbleNPC::Quotes++] = "I am marbling it up.";
	randText[$MarbleNPC::Quotes++] = "I am having a blast.";
	randText[$MarbleNPC::Quotes++] = "I feel like blasting.";
	randText[$MarbleNPC::Quotes++] = "I am rolling all over the place.";
	randText[$MarbleNPC::Quotes++] = "The red gem is strawberry flavored.";
	randText[$MarbleNPC::Quotes++] = "Know thy self, know thy enemy. A thousand battles, a thousand victories.";
	randText[$MarbleNPC::Quotes++] = "To live is to suffer; to survive is to find some meaning in the suffering.";
	randText[$MarbleNPC::Quotes++] = "Happiness resides not in possessions, and not in gems, but in" SPC $pref::highScoreName @ ".";
	//randText[$MarbleNPC::Quotes++] = "Every marble spawns as many, but falls as one.";
	randText[$MarbleNPC::Quotes++] = "The limits of my spherical form are frightening.";
	randText[$MarbleNPC::Quotes++] = "I am the universe experiencing a marble.";
	randText[$MarbleNPC::Quotes++] = "I roll, therefore I am.";
	//randText[$MarbleNPC::Quotes++] = "If I go fast enough, I can escape gravity's judgment.";
	randText[$MarbleNPC::Quotes++] = "Sometimes I roll left (by pressing the A key!) just to feel something.";
	//randText[$MarbleNPC::Quotes++] = "It is not the fall that defines us, but the bounce that follows.";
	randText[$MarbleNPC::Quotes++] = "Faith in the platform is often misplaced.";
	randText[$MarbleNPC::Quotes++] = "Every gem I gather is a piece of myself I did not know was missing.";
	randText[$MarbleNPC::Quotes++] = $pref::highScoreName @ ", my silent companion to guide me on my journey.";
	//randText[$MarbleNPC::Quotes++] = "The unexamined level is not worth loading.";
	//randText[$MarbleNPC::Quotes++] = "They promised a path to enlightenment; instead, I found ice and a duct fan.";
	randText[$MarbleNPC::Quotes++] = "Every marble must roll its own path.";
	randText[$MarbleNPC::Quotes++] = "Naive marbles fear the Tornado." SPC $pref::highScoreName SPC "embraces it.";
	randText[$MarbleNPC::Quotes++] = "There is no checkpoint for the soul; only the Winding Road.";
	//randText[$MarbleNPC::Quotes++] = "I roll unshaken, guided by momentum and madness.";
	randText[$MarbleNPC::Quotes++] = "The marble who yearns for the Blue Gem misses the real treasure:" SPC $pref::highScoreName @ ".";
	randText[$MarbleNPC::Quotes++] = "What cruel god scattered these gems so far apart?";
	randText[$MarbleNPC::Quotes++] = "Every gem I touch feels colder than the last.";
	//randText[$MarbleNPC::Quotes++] = "It wasn’t the fall that broke me. It was the silence after.";
	randText[$MarbleNPC::Quotes++] = "Some say the hazards are tests. Others know they are judgment.";
	//randText[$MarbleNPC::Quotes++] = "The Architects of these paths knew the shape of our despair, and our hope.";
	randText[$MarbleNPC::Quotes++] = "A Time Travel consumed. What future did we just unmake?";
	randText[$MarbleNPC::Quotes++] = "Fortune favors the rolled.";
	randText[$MarbleNPC::Quotes++] = "Carpe gemma; seize the gem.";
	randText[$MarbleNPC::Quotes++] = "A player chooses, a marble obeys.";
	randText[$MarbleNPC::Quotes++] = "The path to salvation is not without bumpers and duct fans.";
	randText[$MarbleNPC::Quotes++] = "Out of bounds, the world resets. I do not.";
	randText[$MarbleNPC::Quotes++] = "I crossed the Bounds trigger once. The timer respawned me - but part of me stayed.";
	//randText[$MarbleNPC::Quotes++] = "Jump too far, and you’ll find it. Not the edge - the truth.";
	//randText[$MarbleNPC::Quotes++] = "The Architects build triggers to keep marbles in. But what were they trying to keep out?";
	//randText[$MarbleNPC::Quotes++] = "I am not a marble, but a pale imitation of one.";
};

function MarbleNPC::make(%this) {
	%client = new GameConnection();
	
	%marb = new Marble() {
		position = vectorAdd($MP::MyMarble.getPosition(), "0 0 1");
		dataBlock = MarbleNPC;
		controllable = false;
		mode = %this.modeFollowPlayer;
		rolling = false;
		jumpAvailable = true;
		shouldScout = true;
		shouldRefreshRandom = true;
		target = $MP::MyMarble;
		targetDistance = 3;
		client = %client;
	};
	//%marb.setCollisionRadius(getRandom(1, 10) / 20);
	%client.setControlObject(%marb);
	%client.player = %marb;
	if(!isObject(MarbleNPCSet)) {
		//echo("not an object");
		
		MissionGroup.add(new SimSet(MarbleNPCSet));
		MarbleNPCSet.add($MP::MyMarble); //TODO?
	}
	%marb.setSync("onDummyRecieved");
	
	if(!isObject(MarbleNPCTagCtrl)) {
		%ctrl = new GuiControl(MarbleNPCTagCtrl) {
			profile = "GuiDefaultProfile";
			horizSizing = "width";
			vertSizing = "height";
			position = "0 0";
			extent = PlayGuiContent.extent;
			minExtent = "8 8";
			visible = "1";
			helpTag = "0";
		};
		PlayGuiContent.add(%ctrl);
		activatePackage(MarbleNPCTagPackage);
	}
}

function onDummyRecieved(%obj) {
	MarbleNPCSet.add(%obj);
	MarbleNPC.update(%obj);
	MarbleNPC.createTag(%obj);
	MarbleNPC.idleTextLoop();
}

function MarbleNPC::respawn(%this, %npc) {
	LocalClientConnection.playPitchedSound("spawn");
	%npc.setVelocity("0 0 0");
	%npc.setAngularVelocity("0 0 0");
	%npc.setTransform(VectorAdd($MP::MyMarble.getTransform(), "0 0 2"));
	%this.update(%npc);
}

function MarbleNPC::onMissionReset(%this, %npc) {
	cancel(%npc.respawnSchedule);
	//%this.respawn(%npc);
}

function MarbleNPC::update(%this, %npc) {
	if (!isObject(%npc) || !isObject($MP::MyMarble) || $Game::Menu)
		return;

	%this.checkCollisions(%npc);
	%this.updateTag(%npc);
	%randomize = true;
	// TODO have a target velocity as well?
	//checkMarbleTriggerCollisions(%npc);
	if(!isObject(%npc.target)) {
		%npc.target = $MP::MyMarble;
		%npc.targetDistance = 3;
	} // TODO
	
	if(%npc.shouldScout)
		%this._scout(%npc);
	
	if(%npc.target != $MP::MyMarble) {
		%this._testPickup(%npc);
	}

	%dir = VectorSub(%npc.target.getPosition(), %npc.getPosition());
	%dist = VectorLen(%dir);
	%rolling = %dist > %npc.targetDistance; // TODO fix this name, maybe minDistance
	%speed = 1;
	if(%dist < 2) {
		%randomize = false;
	}
		
	//%down_amt = %npc.getCollisionRadius() + 0.01;
	%ground_below = ClientContainerRayCast(%npc.getPosition(), VectorSub(%npc.getPosition(), "0 0" SPC "10"), $TypeMasks::InteriorObjectType);
	if(!%ground_below)
		%this.setTagText(%npc, "I'm falling!");
	 
	%look_ahead = VectorScale(%npc.getVelocity(), 0.8); //TODO scalar
	if(VectorLen(%look_ahead) < 1)
		%look_ahead = VectorNormalize(SetWord(%dir, 2, "0"));
	%start = VectorSub(%npc.getPosition(), "0 0" SPC %npc.getCollisionRadius() * 0.75); // Start a bit down the marble
	%end = VectorAdd(%start, %look_ahead);
	%wall_ahead = ClientContainerRayCast(%start, %end, $TypeMasks::InteriorObjectType);
	if(%wall_ahead) {
		// TODO see if there is actual ground to get to (the wall isn't too high)
		// TODO do we actually need this MatrixMulVector stuff to get the normal?
		//%wall_height = ClientContainerRayCast(VectorAdd(%end, "0 0 2"), %end, $TypeMasks::InteriorObjectType);
		//if(%wall_height) {
			%normal = MatrixMulVector(firstWord(%wall_ahead).getTransform(), getWords(%wall_ahead, 4, 6));
			%wall_slope = mRadToDeg(mAcos(VectorDot(%normal, "0 0 1")));
			//echo(%wall_slope);
			if(%wall_slope > 50) {
				%should_jump = true;
				//%npc.spatialText = "There's a wall here!";
				//%this.setTagTextOverride(%npc, "There's a wall!", 100);
			}
		//}

	}
	else {
		// TODO only do this if we are grounded?
		%npc.spatialText = "";
		%gap_pos = "";
		%m_vel = VectorLen(%npc.getVelocity());
		%n_vel = VectorNormalize(%npc.getVelocity());
		%step = %npc.getCollisionRadius();
		//%samples = mCeil(%m_vel / %step);
		%samples = 20;
		for(%i = 0; %i < %samples; %i++) {
			%look_ahead = VectorScale(%n_vel, %i*%step);
			%start = VectorAdd(%npc.getPosition(), %look_ahead);
			%start = VectorAdd(%start, "0 0 1");
			%end = VectorSub(%start, "0 0 3");
			%ground_at_step = ClientContainerRayCast(%start, %end, $TypeMasks::InteriorObjectType);
			if(%ground_at_step) {
				if(%found_gap) {
					%found_landing = true;
					%landing_pos = getWords(%ground_at_step, 1, 3);
					if(VectorDist(%landing_pos, %npc.getPosition()) < %m_vel) {
						%should_jump = true;
						//%npc.spatialText = "I bet I can make this jump...";
						%this.setTagTextOverride(%npc, "I bet I can make this jump...", 200);
						break;
					}
				}
			}
			else {
				%found_gap = true;
				//echo("found the gap");
				if(%gap_pos $= "")
					%gap_pos = VectorAdd(%npc.getPosition(), %look_ahead);
					//%gap_pos = getWords(%ground_at_step, 1, 3);

			}
		}
		if(%found_gap && !%found_landing) {
			//echo("found gap" SPC %gap_pos);
			if(VectorDist(%gap_pos, %npc.getPosition()) < %m_vel) {
				//echo("reversing");
				//%npc.spatialText = "Backing up!";
				//%this.setTagTextOverride(%npc, "Backing up!", 200);
				%should_jump = false;
				%dir = VectorScale(%look_ahead, -1);
				
				%randomize = false;
				%rolling = true;
				%speed = 1;
			}
		}
	}

	if(%should_jump && %npc.jumpAvailable) {
		%this._tryJump(%npc);
	}
	if(%rolling) {
		%this._updateRoll(%npc, %dir, %randomize);
	}
	else {
		%this.setTagText(%npc, "");
	}

	%npc.rolling = %rolling;
	if(VectorLen(%npc.getVelocity()) < 0.1)
		%npc.idleTime += %this.updatePeriod;
	else
		%npc.idleTime = 0;

	if (VectorDist($MP::MyMarble.getPosition(), %npc.getPosition()) > %this.teleportDistance) {
		%npc.respawnSchedule = %this.schedule(%this.respawnDelay, "respawn", %npc);
		alxPlay(OutOfBoundsVoiceSfx);
		%this.setTagTextOverride(%npc, "I went too far...", 500);
		return;
	}

	cancel(%npc.updateSch);
	%npc.updateSch = %this.schedule(%this.updatePeriod, "update", %npc);
}

function MarbleNPC::_scout(%this, %npc) { // TODO save amount of time trying to get to target, switch if too long
	//%npc.scoutText = "";
	if(VectorDist($MP::MyMarble.getPosition(), %npc.getPosition()) > %this.maxStrayDistance) {
		%npc.target = $MP::MyMarble; // TODO make this func?
		%npc.targetDistance = 3;
		//%npc.scoutText = "I went too far!";
		%this.setTagText(%npc, "I'm coming back to" SPC $pref::highScoreName @ "!");
	}
	else if (%npc.target == $MP::MyMarble) {
		InitContainerRadiusSearch($MP::MyMarble.getPosition(), %this.maxStrayDistance, $TypeMasks::ShapeBaseObjectType);
		%min_obj = false;
		%min_dist = 999;
		for (%c_obj = ContainerSearchNext(); isObject(%c_obj); %c_obj = ContainerSearchNext()) {
			//if(%c_obj.getDataBlock().getName() $= "GemItem") {
			//if(%c_obj._huntColor !$= "") {
			if(%c_obj.getClassName() !$= "Item")
				continue;
				
			%cant_see = ClientContainerRayCast(%npc.getPosition(), %c_obj.getPosition(), $TypeMasks::InteriorObjectType);
			%cant_see_2 = ClientContainerRayCast(VectorAdd(%npc.getPosition(), "0 0 1"), %c_obj.getPosition(), $TypeMasks::InteriorObjectType);
			//%cant_see = false;
			if(%cant_see && %cant_see_2)
				continue;
			
			%too_high = (getWord(%c_obj.getPosition(), 2) - getWord(%npc.getPosition(), 2) > 3);
			//echo(getWord(%c_obj.getPosition(), 2) - getWord(%npc.getPosition(), 2));
			
			if(%too_high)
				continue;
			
			%dist = VectorDist(%npc.getPosition(), %c_obj.getPosition());
			if(%dist < %min_dist) {
				%already_targeted = false;
				%steal = false;
				for(%i = 0; %i < MarbleNPCSet.getCount(); %i++) {
					%marb = MarbleNPCSet.getObject(%i);
					if(%marb != %npc && %marb.target == %c_obj) {
						//%npc_dist = VectorDist(%npc.getPosition(), %c_obj.getPosition());
						%teammate_dist = VectorDist(%marb.getPosition(), %c_obj.getPosition());
						if(%dist < %teammate_dist) {
							%steal = true;
							%stole_from = %marb;
						}
						%already_targeted = true;
						break;
					}
				}
				if(!%steal)
					%stole_from = "";
				
				if(!%already_targeted || %steal) {
					%min_obj = %c_obj;
					%min_dist = %dist;
				}
			}
			//}
		}
		if(%min_obj) {
			//if(%already_targeted) {
				if(%stole_from !$= "") {
					%this.setTagText(%stole_from, "I'll let someone else handle this.");
					%stole_from.target = "";
					%msg = "I should steal this";
					echo("Stolen!");
				}
				else
					%msg = "I'll take this";
			//}
			//else
			//	%msg = "I want this";

			
			
			%npc.target = %min_obj;
			%npc.targetDistance = 0;
			//if(%already_targeted)
			//	%msg = "I'll take this";
			//else
			//	%msg = "I want this";
			
			if(%min_obj._huntColor !$= "") {
				%this.setTagText(%npc, %msg SPC %min_obj._huntColor SPC "gem!");
				//%npc.scoutText = %msg SPC %min_obj._huntColor SPC "gem!";
			}
				
			else {
				%name = %min_obj.getDataBlock().useName;
				if(%name $= "")
					%name = "item";
				%this.setTagText(%npc, %msg SPC %name @ "!");
				//%npc.scoutText = %msg SPC %name @ "!";
			}
				
		}
		else {
			if(%already_targeted)
				%this.setTagText(%npc, "Someone's got this covered.");
			else
				%this.setTagText(%npc, "");

				
		}
	}
	else {
		// if no progress is made towards target, scout the area?
		if(%npc.rolling && VectorLen(%npc.getVelocity()) < 0.5) { // TODO don't just base off of velocity, but how much progress we are making
			%npc.target = $MP::MyMarble;
			%npc.targetDistance = 3;
			//%npc.scoutText = "Coming back!";
			%this.setTagText(%npc, "I can't reach it...");
		}
	}
	
	//if(VectorLen(%npc.getVelocity()) < VectorLen(%npc.getAngularVelocity())/8)
		//TODO base this off of a subtraction
		//%this.setTagTextOverride(%npc, "I'm sliding!", 200);

	%npc.shouldScout = false;
	%this.schedule(%this.scoutPeriod, "_refreshScout", %npc);
}

function MarbleNPC::_refreshScout(%this, %npc) {
	%npc.shouldScout = true;
}

function MarbleNPC::_testPickup(%this, %npc) {
	//TODO ensure that the item hasn't been picked up already
	//TODO handle case where player picks up target
	if(%this.boxIntersects(%npc, %npc.target)) {
		if(%npc.target._huntColor !$= "")
			GemItem.onPickup(%npc.target, LocalClientConnection.player, 1);
		
		else if (%npc.target.getDataBlock().className $= "PowerUp") {
			if(%npc.target.getDataBlock().onPickup(%npc.target, %npc, 1)) {
				//if(!isEventPending(%npc.target._respawnSchedule) {
					%dir = VectorSub(%target.getPosition(), %npc.getPosition());
		
					%npc.setCameraYaw(mAtan(getWord(%dir, 0), getWord(%dir, 1)));
					if(%npc.powerUpData.powerUpID)
						%npc.doPowerup(%npc.powerUpData.powerUpID);
					
					%npc.onPowerUpUsed();
				//}
			}
		}
		%this.setTagTextOverride(%npc, "Got it!", 300);
		//%this.setTagText(%npc, "Nothing to grab...");
		%npc.target = $MP::MyMarble;
		%npc.targetDistance = 3;
	}
}

function MarbleNPC::_updateRoll(%this, %npc, %dir, %randomize) {
	%desiredDir = VectorNormalize(%dir);
	
	// Scale by desired speed (based on distance)
	if(%npc.shouldRefreshRandom) {
		%npc.shouldRefreshRandom = false;
		%npc.randomPeriod = getRandom(500, 3000);
		%this.schedule(%npc.randomPeriod, "_refreshRandom", %npc);
	}
	%desiredVel = VectorScale(%desiredDir, %this.targetSpeed);
	if(%randomize) {
		%npc.randomDelta += %this.updatePeriod;
		%completion = %npc.randomDelta / %npc.randomPeriod;
		%random = vectorEase(%npc.oldRandom, %npc.random, %completion);
		%desiredVel = VectorAdd(%desiredVel, VectorScale(%random, %this.randomFactor));
	}
		
	// Compute steering force
	%currentVel = %npc.getVelocity();
	%steeringForce = VectorSub(%desiredVel, %currentVel);
	%angular_dir = -getWord(%steeringForce, 1) SPC getWord(%steeringForce, 0);
	%angular_dir = VectorScale(VectorNormalize(%angular_dir), %this.accelerationSpeed);
	
	// Limit speed influence to avoid overshooting
	%oldAVel = %npc.getAngularVelocity();
	%newAVel = VectorAdd(%oldAVel, %angular_dir);
	
	// Cap the new angular velocity
	if (VectorLen(%newAVel) < %this.maxRollSpeed || VectorLen(%newAVel) < VectorLen(%oldAVel))
		%npc.setAngularVelocity(%newAVel);
	
	if(%npc.idleTime > 1000 && %npc.target == $MP::MyMarble) {
		%this.setTagTextOverride(%npc, "Let's go!", 1000);
	}
}

function MarbleNPC::_tryJump(%this, %npc) {
	%down_amt = %npc.getCollisionRadius() + 0.05;
	%start = %npc.getPosition();
	%can_jump_cast = ClientContainerRayCast(%start, VectorSub(%start, "0 0" SPC %down_amt), $TypeMasks::InteriorObjectType);
	if(!%can_jump_cast) {
		//echo("checking for jump using sampling");
		%samples = 16;
		%offset = 2 / %samples;
		%inc = $pi * (3 - mSqrt(5));
		for(%i = 0; %i < %samples; %i++) {
			%y = %i*%offset - 1 + %offset/2;
			%r = mSqrt(1 - %y*%y);
			%phi = %i * %inc;
			%x = mCos(%phi) * %r;
			%z = mSin(%phi) * %r;
			
			%dir = VectorScale(%x SPC %y SPC %z, %down_amt);
			%end = VectorAdd(%start, %dir);
			%can_jump_cast = ClientContainerRayCast(%start, %end, $TypeMasks::InteriorObjectType);
			if(%can_jump_cast) {
				//echo("jumping from cast" SPC %i);
				break;
			}
		}
	}
	else {
		%pass = 0;
		//echo("can jump from ground");
	}
	if(%can_jump_cast) {
		%normal = MatrixMulVector(firstWord(%can_jump_cast).getTransform(), getWords(%can_jump_cast, 4, 6));
		%jump = VectorScale(VectorNormalize(%normal), 5);
		%npc.applyImpulse(%npc.getPosition(), %jump);
		ServerPlay3D(jumpSfx, %npc.getPosition());

	}
	%npc.jumpAvailable = false;
	%this.schedule(%this.jumpPeriod, "_refreshJump", %npc);
}

function MarbleNPC::idleTextLoop(%this) {
	%idleCount = -1;
	for(%i = 0; %i < MarbleNPCSet.getCount(); %i++) {
		%npc = MarbleNPCSet.getObject(%i);
		if(%npc.idleTime > %this.idleTextWaitTime && %npc.tag.plaintext $= "")
			%idleMarbles[%idleCount++] = %npc;
	}
	%npc = %idleMarbles[getRandom(0, %idleCount)];
	if(isObject(%npc))
		%this.setTagTextOverride(%npc, %this.randText[getRandom(0, $MarbleNPC::Quotes)], %this.idleTextDuration);
	cancel(%this.idleTextLoop);
	%this.idleTextLoop = %this.schedule(getRandom(%this.idleTextPeriodMin, %this.idleTextPeriodMax), "idleTextLoop");
}

function MarbleNPC::_refreshRandom(%this, %npc) {
	%npc.randomDelta = 0;
	%npc.random = getRandom()-0.5 SPC getRandom()-0.5 SPC 0;
	%npc.shouldRefreshRandom = true;
}

function MarbleNPC::_refreshJump(%this, %obj) {
	%obj.jumpAvailable = true;
}

function MarbleNPC::boxIntersects(%this, %marb, %obj) { // TODO getCollisionBox?
	return boxIntersectionTest(%marb.getWorldBox(), %obj.getWorldBox());
}

function MarbleNPC::createTag(%this, %npc) {
	%len = 0;
	%gui = new GuiMLTextCtrl() {
		profile = "GuiDefaultProfile";
		position = "0 0";
		extent = %len SPC "24";
		minExtent = "8 8";
		defaultLength = %len;

		new GuiBitmapCtrl() {
			profile = "GuiDefaultProfile";
			position = "0 4";
			extent = %len SPC "18";
			minExtent = "8 8";
			bitmap = $NameTag::BackGround;
		};
	};
	MarbleNPCTagCtrl.add(%gui);
	%gui.background = %gui.getObject(0);
	%npc.tag = %gui;
	%this.setTagText(%npc, "blah");
}

function MarbleNPC::setTagTextOverride(%this, %npc, %text, %time) {
	if(!isObject(%npc.tag))
		return;
	if(%text $= %npc.tag.getText())
		return;
	cancel(%npc.liftSch);
	%this.setTagText(%npc, %text);
	%npc.tag.override = true;
	%npc.tag.overrideText = %npc.tag.getText();
	%npc.liftSch = %this.schedule(%time, "_liftTextOverride", %npc);
}

function MarbleNPC::_liftTextOverride(%this, %npc) {
	%npc.tag.override = false;
	if(%npc.tag.getText() $= %npc.tag.overrideText)
		%this.setTagText(%npc, "");
}

function MarbleNPC::setTagText(%this, %npc, %text) {
	if(%npc.tag.override) {
		//%npc.tag.nextText = %text;
		return;
	}

	if(%text $= %npc.tag.getText())
		return;
	
	if(!isObject(%npc.tag))
		return;
	%npc.tag.plaintext = %text;
	%npc.tag.setVisible((%text !$= ""));
	%len = textLen(stripMLControlChars(%text), $DefaultFont, 22) + 8;
	//if(getRecordCount(%text) == 2) {
	//	%npc.tag.extent = %len SPC 24*2;
	//	%npc.tag.background.extent = %len SPC 18*3;
	//}
	//else {
		%npc.tag.extent = %len SPC 24;
		%npc.tag.background.extent = %len SPC 18;
	//}

	%npc.tag.defaultLength = %len;

	%npc.tag.setText("<just:center><font:22><color:ffffff>" @ %text);
}

function MarbleNPC::updateTag(%this, %npc) {
	%tag = %npc.tag;
	if(!isObject(%tag))
		return;
	// player position
	%pos = %npc.getWorldBoxCenter();

	// make it above the marble
	%cameraTrans = getCameraTransform();

	%distance = vectorDist(MatrixPos(%cameraTrans), %npc.getPosition());
	%pos = vectorAdd(%pos, VectorScale(getGravityDir(), -2 * %npc.getCollisionRadius()));

	//Just pick something really large as default so they go the whole way
	//%maxDist = ClientMode::callback("nametagDistance", 2000); //Should be ~draw distance
	//if (%distance > %maxDist) {
	//	%tag.setVisible(false);
	//	//echo("out of range");
	//	return;
	//}

	%screenPos = getGuiSpace(%cameraTrans, %pos, getCameraFov());
	// are we off the screen?
	//if (isOffScreen(%screenPos)) {
	//	%tag.setVisible(false);
	//	return;
	//}

	// see if the player is hidden. If it is, don't display the nametag.
	// we delay raycast so that we only have to draw the ray if the marbles
	// are in the viewport =)
	//
	// Please note, fast mode does not do this check. (This could get laggy
	// for people with bad CPUs, all these raycasts!)
	//%doCast = ClientMode::callback("nametagRaycast", true);
	//if (!$pref::FastMode && %doCast) {
	//	// do not put an exeption in clientContainerRaycasting, I think its causing
	//	// virtual memory errors......
	//	if (clientContainerRayCast(%cameraTrans, %pos, $NameTag::TypeMask)) {
	//		%tag.setVisible(false);
	//		return;
	//	}
	//}

	%extent = %tag.defaultLength SPC "18";

	%screenPos = VectorClamp(%screenPos, -1, 1);
	%screenPos = getPixelSpace(%screenPos);
	%screenPos = VectorClampGui(%screenPos, 8);
	%screenPos = VectorRound(VectorSub(%screenPos, VectorScale(%extent, 0.5)));

	%tag.setPosition(%screenPos);
	//%tag.setVisible(true);
	
	//if (RootGui.getContent().getName() $= "PlayGui")
	if(%tag.isVisible())
		%tag.forceReflow();
}

function Marble::applyVelocity(%this,%pos,%vec,%ignoreFreeze) {
	 //if (%ignoreFreeze || !%this.transformFrozen) 
		 //echo("applygin");
		 %this.applyImpulse(%pos,VectorScale(%vec,%this.getDatablock().mass));
}

function MarbleNPC::checkCollisions(%this, %npc) { // Credits: Whirligig
	%count = MarbleNPCSet.getCount();
	for (%i = 0; %i < %count+1; %i ++) {
		%npc2 = %i == %count ? $MP::MyMarble : MarbleNPCSet.getObject(%i);
		if(%npc2 == %npc)
			continue;
		
		%pos1 = %npc.getPosition();
		%pos2 = %npc2.getPosition();
		%v = VectorDist(%pos1,%pos2);
		%rad1 = %npc.getCollisionRadius();
		%rad2 = %npc2.getCollisionRadius();
		%dist = VectorLen(%v);
		%is = (%dist <= %rad1+%rad2) && (%dist >= 0.1) && (%oldDist < 5);
		if (%is && !%this.cantCollide[%npc,%npc2]) {
			%this.cantCollide[%npc,%npc2] = true;
			%sgnVec = VectorNormalize(VectorSub(%pos1,%pos2));
			%vel2 = %npc2.getVelocity();
			%vel1 = %npc.getVelocity();
			
			// Impulse
			%rest = 0.0125;
			%newVel2 = VectorAdd(VectorScale(VectorAdd(VectorScale(%sgnVec,2*VectorDot(%vel2,VectorScale(%sgnVec,-1))),%vel2),%rest),%vel1);
			%newVel1 = VectorAdd(VectorScale(VectorAdd(VectorScale(%sgnVec,-2*VectorDot(%vel1,%sgnVec)),%vel1),%rest),%vel2);
			%npc2.applyVelocity("0 0 0",VectorSub(%newVel2,%vel2));
			%npc.applyVelocity("0 0 0",VectorSub(%newVel1,%vel1));

			// FX
			%b1 = eval ("return" SPC %npc.getDatablock() @ ".bounce" @ getRandom(1, 4) @ ";");
			%b2 = eval ("return" SPC %npc2.getDatablock() @ ".bounce" @ getRandom(1, 4) @ ";");
			if (VectorDist(%newVel1,%vel1) > 3 || VectorDist(%newVel2,%vel2) > 3) {
				if (%npc2 == $MP::MyMarble) {
					if(VectorLen(%vel1) < VectorLen(%vel2))
						%this.setTagTextOverride(%npc, "Ouch!", 500);
					else
						%this.setTagTextOverride(%npc, "Sorry!", 500);
				}
				else
					%this.setTagTextOverride(%npc, "Ow!", 500);
				alxPlay(%b1,getWord(%npc.getPosition(),0),getWord(%npc.getPosition(),1),getWord(%npc.getPosition(),2));
				spawnEmitter(100, "MarbleBounceEmitter", VectorScale(VectorAdd(%pos1,%pos2), 0.5));
			}
		}
		else if (%is) {
			%sgnVec = VectorNormalize(VectorSub(%pos1,%pos2));
			%vel2 = "";
			%vel1 = "";
			%rest2 = 0;
			%rest1 = 0;
			%newVel2 = VectorAdd(VectorScale(VectorAdd(VectorScale(%sgnVec,2*VectorDot(%vel2,VectorScale(%sgnVec,-1))),%vel2),%rest2*%rest1),%vel1);
			%newVel1 = VectorAdd(VectorScale(VectorAdd(VectorScale(%sgnVec,-2*VectorDot(%vel1,%sgnVec)),%vel1),%rest2*%rest1),%vel2);
			%npc2.applyImpulse("0 0 0",VectorSub(%newVel2,%vel2));
			%npc.applyImpulse("0 0 0",VectorSub(%newVel1,%vel1));
			%npc.applyImpulse("0 0 0",VectorScale(%sgnVec,0.5));
			%npc2.applyImpulse("0 0 0",VectorScale(%sgnVec,-0.5));
		}
		else {
			%this.cantCollide[%npc,%npc2] = false;
		}
	}
}

package MarbleNPCTagPackage {
	function GameConnection::onClientLeaveGame(%this) {
		Parent::onClientLeaveGame(%this);
		if(isObject(MarbleNPCTagCtrl))
			MarbleNPCTagCtrl.delete();
		deactivatePackage(MarbleNPCTagPackage);
	}
};