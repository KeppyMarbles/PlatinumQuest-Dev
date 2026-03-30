// learning to troll
// Wall at start pad, go on trim
// Wr path sends finish sign to hit marble
// Must hold one button
// tunnel into different level

// Gem appears right as the finish pad is available

// Explode marble: make landmine, on next frame delete and hide marble, toggle camera to last camera pos and lock keys

// dimensional anamoly

// Gem that moves on a random path away from marble
// Move based on spin?
// Can only collect if moving slow enough?

// fake out finish sequence

// super jump turns into shock absorber 

// choose wisely - set of powerups (they actually give a completely different powerup)

// fake gems (explode on touch)
// maybe have some sea of them (red gems) and you have to collect the blue gems around
// or maybe the fake ones spin in the wrong direction

// Make focus on grabbing gems instead of getting to finish pad?
// Turn the gravity more the closer you get to the finish pad / gem
// Instant teleport trigger on the finish pad / gem
// Timed teleport trigger where you have to find the secret in time
// Sequence memory - you have to memorize some order at the beginning and press buttons correctly
// For the bumper fireball, have it be a meteor that falls from the sky and makes an explosion

// gems that make hazards when you collect them 

// runaway gem: raycast at the bottom, in the direction that it's moving to hit the trim, and if hits close enough, try moving in the direction parallel to the surface (choose left or right based on angle)
// drop a dummy marble that's able to collect it? and maybe add more gems to push
// the dummy marble is in some spot where you have to push it off
// make it involved, maybe having to push multiple dummies towards some goal

// caged gem? two buttons to open

// Randomly generated simon says sequence

// Gem which stays behind the camera

// Colmesh maze

// Tile which lowers into a maze inside of the interior

// Actual finish should just be difficult to find
// Maybe a secret tile to grab powerups

//i'm creating a marble blast level that's disguised as the first training level, but it actually keeps doing stuff to challenge or "troll" the player:
//1. A gem spawns behind you right as you get to the finish, preventing you from finishing since you have to go collect it.
//2. Another gem spawns when you collect that one, behind you assuming that your camera was facing away from it.
//3. When collecting that gem, the finish and start pads swap places out of your view, confusing you when you turn around.
//4. When you enter the finish pad, it does the finish fireworks and stuff, making you think you finished the level but then it stops.
//5. In the process, a platform underneath the level spawns, with a gem on it.
//6. when you use a super jump, the level turns into another level, Bumper Training, which has the same geometry but a different color. You can't see it changing color when jumping up, so it's funny/surprising to see.
//7. Instead of normal bumpers, they are actually coded to track and quickly move towards the player trying to knock them off the map.
//8. To counter this you pick up the fireball powerup so that they explode when they try to knock you off.
//9. The map changes back to the original training level when they're all destroyed.
//
//And here's where i'm stuck. i need some ideas for continuing the sequence.




datablock AudioProfile(swooshSfx1) {
	filename = "~/data/sound/custom/482880__brsjak__swoosh1.wav";
	description = AudioDefault3d;
	preload = true;
};
datablock AudioProfile(swooshSfx2) {
	filename = "~/data/sound/custom/482880__brsjak__swoosh2.wav";
	description = AudioDefault3d;
	preload = true;
};
datablock AudioProfile(swooshSfx3) {
	filename = "~/data/sound/custom/482880__brsjak__swoosh3.wav";
	description = AudioDefault3d;
	preload = true;
};

package fakePad {
	function GameConnection::onEnterPad(%this) {
		if (Mode::callback("onEnterPad", false, new ScriptObject() {
			client = %this;
			_delete = true;
		}))
			return;
	
		//Don't let us finish twice
		if ($Game::Finished) {
			return;
		}
	
		if (%this.player.getPad() == $Game::EndPad) {
			echo("GemCount is" SPC %gemCount SPC "(user)," SPC $Game::GemCount SPC "(game)");
	
			%message = "Congratulations! You've finished! Probably!";
			if (%this.canFinish()) {
				%this.player.setMode(Victory);
				if (%message !$= "") {
					messageClient(%this, 'MsgRaceOver', %message);
				}
				$Game::FinishClient = %this;
				// add to cleanup
				
				schedule(4000, 0, "eval", "ltrSequenceTrigger::onEnterTrigger(0, platformSequence," SPC %this.player @ ");");
				startFireWorks($Game::EndPad);
				%this.playPitchedSound("firewrks");
				
			} else {
				%this.playPitchedSound("missinggems");
				if (%message !$= "") {
					messageClient(%this, 'MsgMissingGems', %message);
				}
			}
		}
		//deactivatePackage(fakePad);
	}
};

package explodePad {
	function GameConnection::onEnterPad(%this) {
		explodemarble();
	}
};

package tempGems {
	function Gem::onPickup(%this,%obj,%user,%amount) {
		Parent::onPickup(%this,%obj,%user,%amount);
		%obj.delete();
	}
};


function TempEmitter(%pos, %type, %life) {
	%emitter = new ParticleEmitterNode() {
		position = %pos;
		dataBlock = fireWorkNode;
		emitter = %type;
	};
	%emitter.schedule(%life, "delete");
}

//function testMine() {
//	%mine = new StaticShape() {
//		position = $MP::MyMarble.position;
//		scale = "1 1 1";
//		dataBlock = LandMine;
//	};
//	//%mine.setDamageState("Destroyed");
//	%mine.schedule(500, "setDamageState", "Destroyed");
//	//%mine.schedule(100, "delete");
//	//%obj.delete();
//}

package BumperFireball {
	function Bumper::onCollision(%this, %obj, %col) {
		if($Client::FireballActive) {
			TempEmitter(%obj.position, "LandMineSmokeEmitter", 500);
			ServerPlay3D(ExplodeMineSfx, %obj.position);
			%obj.delete();
			%col.setVelocity(VectorScale(%col.getVelocity(), 0.5));
		}
		else {
			Parent::onCollision(%this, %obj, %col);
		}
	}
};
activatePackage(BumperFireball);
//$Client::FireballActive
//function ConsoleEntry::eval () {
//	%text = ConsoleEntry.getValue();
//	if (%text $= "")
//		return;
//
//	echo("\c3$ " @ %text);
//
//	eval (%text);
//	ConsoleEntry.setValue("");
//
//	ConsoleTrace.setValue($tracing);
//	ConsoleDebug.setValue($DEBUG);
//	ConsoleEEnabled.setValue($Editor::Enabled);
//}

datablock triggerData(ltrSequenceTrigger) {
	customField[0, "field"] = "sequence";
	customField[0, "type"] = "string";
	tickPeriodMS = 100;
};

//create new help trigger that deletes itself on use

// function HelpTrigger::onEnterTrigger() {
	//Parent::OnEnterTrigger();
	//%trigger.delete();
//}

$triggerPolyhedron = "0.0000000 0.0000000 0.0000000 1.0000000 0.0000000 0.0000000 0.0000000 -1.0000000 0.0000000 0.0000000 0.0000000 1.0000000";

//runLtrSequence("removeLowerPlatform");
//runLtrSequence("bumpersDestroyed");
//runLtrSequence("runawayGem");
function runLtrSequence(%toSeq) {
	for(%i = 0; %i < sequenceList.count; %i++) {
		%seq = sequenceList.name[%i];
		ltrLevel.schedule(200*%i, "activateSequence", %seq);
		if(%seq $= %toSeq)
			return;
	}
}

function KeppyLTR::activateSequence(%this, %sequence) {
	echo("activating seq" SPC %sequence);
	switch$ (%sequence) { 
		// call on fake pad
		case "createGem":
			ltrCleanup.add(new SimGroup(createGemSequence) {
				new Item() {
					position = "0 6 499.578";
					dataBlock = "GemItemTurquoise";
					static = 1;
					rotate = 1;
				};
				new Trigger() {
					position = "-0.5 6.5 499.355";
					dataBlock = "ltrSequenceTrigger";
					polyhedron = $triggerPolyhedron;
						Sequence = "createGem2";
				};
				new Trigger() {
					position = "12.25 28.7043 498.9";
					scale = "1 9.05654 3.4642";
					dataBlock = "HelpTrigger";
					polyhedron = $triggerPolyhedron;
						displayonce = "1";
						persistTime = "5000";
						text = "Kinda hilarious that you forgot a gem that was literally right in front of you.";
				};
			});
			$Game::GemCount = 1;
			//PG_GemCounter.setVisible(true);
		case "createGem2":
			ltrCleanup.add(new SimGroup(createGem2Sequence) {
				new Item() {
					position = "8 27 499.578";
					dataBlock = "GemItemTurquoise";
					static = 1;
					rotate = 1;
				};
				new Trigger() {
					position = "7.5 27.5 499.355";
					dataBlock = "ltrSequenceTrigger";
					polyhedron = $triggerPolyhedron;
							Sequence = "finishSwap";
				};
				new Trigger() {
					position = "7.25 28.75 498.9";
					scale = "1 9.05654 3.4642";
					dataBlock = "HelpTrigger";
					polyhedron = $triggerPolyhedron;
						displayonce = "1";
						persistTime = "5000";
						text = "Oh, yeah. You forgot this one too.";
				};
			});
		//$Game::GemCount = 2;
			
		case "finishSwap":
			sign1.delete();
			sign2.delete();
			StartPoint.setTransform("24.0431 8.5256 499.43");
			EndPoint.setTransform("0.0682294 0.50582 499.353");
			EndSign.setTransform("1.28547 0.764379 504.46 0 0 1 0.523599");
			activatePackage(fakePad);
			// TODO Message if start and finish are in view
			
			ltrCleanup.add(new SimGroup(finishSwapSequence) {
				new StaticShape(sign1) {
					position = "27.8874 23.868 500.519";
					rotation = "-0.0993088 0.0943882 0.99057 87.6319";
					dataBlock = "SignPlainLeft";
				};
				new StaticShape(sign2) {
					position = "-0.600503 28.231 500.524";
					rotation = "0 0 -1 8.02137";
					dataBlock = "SignPlainLeft";
				};
				new Trigger(platformSequence) {
					position = "0 0 0";
					dataBlock = "ltrSequenceTrigger";
					polyhedron = $triggerPolyhedron;
							Sequence = "createLowerPlatform";
				};
				//new Trigger() {
				//	position = "19.5 17.625 499.451";
				//	scale = "9.04967 0.773525 3.4642";
				//	dataBlock = "HelpTrigger";
				//	polyhedron = $triggerPolyhedron;
				//		displayonce = "1";
				//		persistTime = "5000";
				//		text = "Wrong way...";
				//};
			});
			
		case "createLowerPlatform":
			// finish pad broke
			// you forgot another gem
			
			
			createGemSequence.delete();
			createGem2Sequence.delete();
			
			LocalClientConnection.player.setMode("Normal");
			ltrCleanup.add(new SimGroup(CreateLowerPlatformSequence) {
				new Item() {
					position = "24 5 495.328";
					dataBlock = "GemItemTurquoise";
					static = 1;
					rotate = 1;
				};
				new InteriorInstance(platform) {
					position = "21.0023 8.33873 495.238";
					interiorFile = "platinum/data/interiors_mbg/advanced/trapdoor.dif";
				};
				new Item(platformSJ) {
					position = "24 3.5 495.508";
					dataBlock = "SuperJumpItem";
					static = 1;
					rotate = 1;
				};
				new Trigger() {
					position = "21 8.25 495.163";
					scale = "6.13961 6.26427 1.6294";
					dataBlock = "ltrSequenceTrigger";
					polyhedron = $triggerPolyhedron;
							Sequence = "bumperTraining";
				};
			});
			
			//%mine = new StaticShape() {
			//	scale = "0 0 0";
			//	position = EndPoint.position;
			//	dataBlock = LandMine;
			//};
			TempEmitter(EndPoint.position, "LandMineSmokeEmitter", 500); // TODO
			ServerPlay3D(ExplodeMineSfx, EndPoint.position);
			EndPoint.setTransform("0 0 0");
			//CreateLowerPlatformSequence.add(%mine);
			//%mine.setDamageState("Destroyed");
			
		case "bumperTraining":
			ltr.delete();
			finishSwapSequence.delete();
			//sign1.delete();
			//sign2.delete();
			//EndPoint.setTransform(vectorAdd("12 -4 0.334106 0 0 1 3.15318164", "12.0128 12.1969 499.11"));
			//EndSign.setTransform(vectorAdd("12.3983 -3.68577 6.35636 0 0 -1 0.34", "12.0128 12.1969 499.11"));
			//StartPoint.setTransform(vectorAdd("-12 -12 0.339511", "12.0128 12.1969 499.11"));
			
			//todo fireball should land as a meteor
			
			EndSign.setTransform("0 0 0");
			EndPoint.setTransform("0 0 0");
			
			ltrCleanup.add(new SimGroup(bumperTrainingSequence) {
				//new SimGroup(BumperGroup);
				new InteriorInstance(bumperTraining) {
					position = "12.0128 12.1969 499.11";
					rotation = "1 0 0 0";
					scale = "1 1 1";
					interiorFile = "~/data/interiors_mbg/beginner/training_bumpers.dif";
					showTerrainInside = "0";
							locked = "true";
				};
				new Trigger() {
					position = "20.25 17.0242 498.75";
					scale = "7.78272 7.00054 2.99585";
					dataBlock = "ltrSequenceTrigger";
					polyhedron = $triggerPolyhedron;
							Sequence = "removeLowerPlatform";
				};
				new Trigger() {
					position = "17.25 28.7043 498.9";
					scale = "8.54803 9.05654 3.4642";
					dataBlock = "HelpTrigger";
					polyhedron = $triggerPolyhedron;
						displayonce = "1";
						persistTime = "5000";
						text = "You know that level Bumper Training? Well, I was actually training the BUMPERS, not you.";
				};
				new Item() {
					position = "0 10.25 500";
					dataBlock = FireballItem;
					collideable = 0;
					static = 1;
					rotate = 1;
					activeTime = 15000;
				};
			});
			
			%bumpers = new SimGroup();
			bumperTrainingSequence.add(%bumpers);
			//for(%pos = bumperList.pos[0]; %pos !$= ""; %pos = bumperList.pos[%i++]) {
			for(%i = 1; %i <= bumperList.count; %i++) {
				%bumpers.add(%bumper = new StaticShape() {
					position = vectorAdd(bumperList.pos[%i], bumperTraining.position);
					dataBlock = "roundBumper";
				});
				%bumper.center = %bumper.getWorldBoxCenter();
			}
			%this.updateBumpers(%bumpers);
			
		
		case "removeLowerPlatform":
			createLowerPlatformSequence.delete();
			//platform.delete();
			//platformSJ.delete();
			
		case "bumpersDestroyed":
			//todo cancel the fireball
			bumperTrainingSequence.delete();
			new InteriorInstance(ltr) {
					position = "12.0128 12.1969 499.11";
					rotation = "1 0 0 0";
					scale = "1 1 1";
					interiorFile = "~/data/interiors_mbg/beginner/training1.dif";
					showTerrainInside = "0";
						locked = "true";
			};
			ltrCleanup.add(new SimGroup(GravityGemSequence));
			%gems = new SimGroup() {
				new Item() {
					position = "0 6 499.578";
					dataBlock = "GemItemTurquoise";
					static = 1;
					rotate = 1;
				};
				new Item() {
					position = "24 14.25 499.504";
					dataBlock = "GemItemTurquoise";
					static = 1;
					rotate = 1;
				};
			};
			GravityGemSequence.add(%gems);
			%this.updateGravity(%gems);
			
		case "runawayGem":
			GravityGemSequence.delete();
			ltrCleanup.add(new SimGroup(RunawayGemSequence));
			%gems = new SimGroup() {
				new Item() {
					position = "10 24.25 499.578";
					dataBlock = "GemItemTurquoise";
					static = 1;
					rotate = 1;
				};
			};
			RunawayGemSequence.add(%gems);
			%this.updateGemSpace(%gems);
		
		case "placeholder":
			RunawayGemSequence.delete();
			return;
	}
}

function ltrSequenceTrigger::onEnterTrigger(%this, %trigger, %user) {
	ltrLevel.activateSequence(%trigger.sequence);
	%trigger.delete();
}

function KeppyLTR::updateBumpers(%this, %group) {
	if(!isObject(%group))
		return;
	// todo: all clients
	%pos = localClientConnection.player.getPosition();
	for(%i = 0; %i < %group.getCount(); %i++) {
		%bumper = %group.getObject(%i);
		if(VectorDist(%pos, %bumper.center) < 1.5)
			objHitMarble(localClientConnection.player, %bumper, 300);
	}
	if(%group.getCount() == 0)
		%this.activateSequence("bumpersDestroyed");
	else
		%this.schedule(100, "updateBumpers", %group);
}

//function clientCbOnFrameAdvance() {
//	if(isObject(GravityGroup)) {
//		updateGravity();
//	}
//	updateGemPos();
//}

//function KeppyLTR::onFrameAdvance(%this) {
//	%this.updateGravity();
//	%this.updateGemSpace();
//}

function KeppyLTR::updateGemSpace(%this, %group) { // TODO play a noise
	if(!isObject(%group))
		return;
		
	if(%group.getCount() < 1) {
		%this.activateSequence("placeholder");
		return;
	}
		
	%playerPos = $MP::MyMarble.getPosition();
	for(%i = 0; %i < %group.getCount(); %i++) {
		%gem = %group.getObject(%i);
		%gemPos = %gem.getPosition();
		%dist = VectorDist(%playerPos, %gemPos);
		if(%dist > 2)
			continue;
		%check = ClientContainerRayCast(%gemPos, VectorSub(%gemPos, "0 0 1"), $TypeMasks::InteriorObjectType);
		if(!%check)
			continue; //TODO put gem in a better spot?
		%playerPos = setWord(%playerPos, 2, getWord(%gemPos, 2));
		%offset = vectorSub(%gemPos, %playerPos);
	
		// Compute desired direction and target offset (2 units away)
		%angle = mAtan(getWord(%offset, 1), getWord(%offset, 0));
		%targetOffset = 2 * mCos(%angle) SPC 2 * mSin(%angle) SPC 0;
		%targetPos = vectorAdd(%playerPos, %targetOffset);
		%targetCast = vectorAdd(%playerPos, VectorScale(%targetOffset, 1.2));
	
		// Raycast from gem to target position to detect wall obstruction
		%rayStart = vectorSub(%gemPos, "0 0" SPC getRadius("z", %gem) - 0.125); // Slight vertical offset
		%rayEnd = vectorSub(%targetCast, "0 0" SPC getRadius("z", %gem) - 0.125);
	
		%hit = ClientContainerRayCast(%rayStart, %rayEnd, $TypeMasks::InteriorObjectType);
	
		if (!%hit) {
			// No wall in the way: move directly to target position
			%targetPos = setWord(%targetPos, 2, getWord(%gemPos, 2)); // keep current Z
			%gem.setTransform(%targetPos);
		}
		else {
			// Wall in the way: slide along wall surface
	
			// Step 1: Direction to move
			%moveDir = vectorNormalize(vectorSub(%targetPos, %gemPos));
	
			// Step 2: Get wall normal from raycast hit
			%normal = MatrixMulVector(%hit.getTransform(), getWords(%hit, 4, 6));
			//%normal = getWords(%hit, 4, 6);
			%normal = vectorNormalize(%normal);
	
			// Step 3: Slide movement: project movement onto plane perpendicular to wall normal
			%dot = VectorDot(%moveDir, %normal);
			%slideDir = vectorNormalize(vectorSub(%moveDir, vectorScale(%normal, %dot)));
	
			// Step 4: Move a small step in the slide direction (prevents jumping/teleporting)
			//echo(%dist - 0.75);
			%stepSize = 0.05 / (%dist - 0.75); // Adjust this for smoothness
			%newPos = vectorAdd(%gemPos, vectorScale(%slideDir, %stepSize));
			%newPos = setWord(%newPos, 2, getWord(%gemPos, 2)); // Keep original Z
	
			%gem.setTransform(%newPos);
		}
	}

	%this.schedule(20, "updateGemSpace", %group);
}

function KeppyLTR::updateGravity(%this, %group) {
	if(!isObject(%group))
		return;
	if(%group.getCount() < 1) {
		%this.activateSequence("runawayGem");
		return;
	}
		
	%pos = LocalClientConnection.player.getPosition();
	%change = false;
	for(%i = 0; %i < %group.getCount(); %i++) {
		%point = %group.getObject(%i).getWorldBoxCenter();
		%offset = vectorSub(%pos, %point);
		%dist = vectorLen(%offset);
		if(%dist < 10) {
			%change = true;
			
			%deg = 90 - (%dist * 9);
			%angle = mAtan(getWord(%offset, 1), getWord(%offset, 0));
			%rot1 = "1 0 0" SPC mDegToRad(%deg);
			%rot2 = "0 0 -1" SPC (%angle + $pi / 2);
			%rot3 = RotMultiply(%rot2, %rot1);
			
			%finalRot = RotMultiply(%rot3, "1 0 0" SPC $pi); 
			%ortho = vectorOrthoBasis(%finalRot);
			LocalClientConnection.setGravityDir(%ortho, false, %finalRot);
			
			
			LocalClientConnection.player.changedGravity = true;
		}
	}
	if(!%change && LocalClientConnection.player.changedGravity) {
		LocalClientConnection.setGravityDir("1 0 0 0 -1 0 0 0 -1", false, "1 0 0" SPC $pi);
		LocalClientConnection.player.changedGravity = false;
		schedule(1500, 0, "fixGravity");
	}
	%this.schedule(20, "updateGravity", %group);
}

function fixGravity() {
	if(!LocalClientConnection.player.changedGravity) {
		LocalClientConnection.setGravityDir("1 0 0 0 -1 0 0 0 -1", true, "1 0 0" SPC $pi);
	}
}

datablock triggerData(explodeMarbleTrigger) {
	tickPeriodMS = 100;
};

function KeppyLTR::onMissionReset(%this) {
	%this.delete();
	%this = new ScriptObject(ltrLevel) {
		class = "KeppyLTR";
	};
	MissionGroup.add(%this);
	deactivatePackage(fakePad);
	StartPoint.hide(false);
	StartPoint.setTransform("0.0682294 0.50582 499.353");
	ltrInit.delete();
	new SimGroup(ltrInit) {
	 
		new InteriorInstance(ltr) {
			position = "12.0128 12.1969 499.11";
			rotation = "1 0 0 0";
			scale = "1 1 1";
			interiorFile = "~/data/interiors_mbg/beginner/training1.dif";
			showTerrainInside = "0";
					locked = "true";
		};
		
		new StaticShape(sign1) {
			position = "27.8874 23.868 500.519";
			rotation = "-0.0993088 0.0943882 0.99057 87.6319";
			dataBlock = "SignPlainRight";
		};
		new StaticShape(sign2) {
			position = "-0.600503 28.231 500.524";
			rotation = "0 0 -1 8.02137";
			dataBlock = "SignPlainRight";
		};
		
		new StaticShape(EndSign) {
			position = "22.4672 8.43532 504.46";
			rotation = "0 0 1 160.519";
			dataBlock = "SignFinish";
		};
		
		new Trigger() {
			position = "-4.5 8 499.451";
			scale = "9.04967 0.773525 3.4642";
			dataBlock = "HelpTrigger";
			polyhedron = $triggerPolyhedron;
				displayonce = "1";
				persistTime = "5000";
				text = "You're learning to roll again...? There's literally only 4 buttons.";
		};
		
		new Trigger() {
			position = "21.75 11 499.355";
			scale = "4.73914 4.23789 5.22739";
			dataBlock = "ltrSequenceTrigger";
			polyhedron = $triggerPolyhedron;
					Sequence = "createGem";
		};
		new StaticShape(EndPoint) {
				position = "24.0431 8.5256 499.43";
				rotation = "0 0 1 179.518";
				dataBlock = "EndPad_MBG";
		};
		
	};
	
	ltrCleanup.clear();
	
	//fix
	PG_GemCounter.setVisible(false);
}

//function createGemTrigger::onEnterTrigger(%this, %trigger, %user) {
//
//}

function explodeMarbleTrigger::onEnterTrigger(%this, %trigger, %user) {
	explodeMarble(%user);
}

//new actionMap(explode);
//explode.bindCmd(mouse, "button0", "explodeMarble();", "");

function explodeMarble(%marb) {
	if(%marb $= "")
		%marb = localClientConnection.player;
	
	%mine = new StaticShape() {
		position = %marb.position;
		scale = "0 0 0";
		datablock = "LandMine";
	};
	schedule(100, 0, "eval", %mine @ ".delete();");
	$MP::MyMarble.setScale("0 0 0");
	%marb.client.freezeMarble(true);

	//%sch = %marb.client.schedule(0, "onOutOfBounds");
	%marb.client.onOutOfBounds();
	eval ("package explodemarb { function clientCmdCbOnRespawn() { Parent::clientCmdCbOnRespawn();" @ $MP::MyMarble @ ".setScale(\"1 1 1\"); deactivatePackage(explodemarb); } };");
	activatePackage(explodemarb);
	
}
datablock triggerData(hitMarbleTrigger) {
	tickPeriodMS = 100;
	customField[0, "field"] = "object";
	customField[0, "type"] = "string";
	customField[0, "desc"] = "The object to hit the marble with.";
	customField[1, "field"] = "time";
	customField[1, "type"] = "numeric";
	customField[1, "desc"] = "The time it takes to get there.";
};

function hitMarbleTrigger::onEnterTrigger(%this, %trigger, %user) {
	if(%trigger.inProgress)
		return;
	%trigger.inProgress = true;
	schedule(%trigger.time*2, 0, "eval", %trigger @ ".inProgress=false;");
	objHitMarble(%user, %trigger.object, %trigger.time);
}

// Just move existing pathnodes if only used for finish sign

// function SimObject::hitObject
function objHitMarble(%marb, %obj, %time) {
	//%marb = localClientConnection.player;
	if(%obj._moving)
		return;
	ServerPlay3D("swooshSfx" @ getRandom(1, 3), %obj.getWorldBoxCenter());
	%futurePos = vectorAdd(%marb.position, vectorScale(%marb.getVelocity(), %time/1000));
	%futurePos = vectorScale(vectorAdd(%futurePos, %marb.position), 0.5);
	%node1 = new StaticShape() {
		position = %obj.position;
		rotation = %obj.rotation;
		dataBlock = "PathNode";
		timeToNext = %time;
			Smooth = "1";
			useScale = "0";
	};
	%node2 = new StaticShape() {
		position = %futurePos;
		rotation = %obj.rotation;
		dataBlock = "PathNode";
		timeToNext = %time;
			Smooth = "1";
			useScale = "0";
	};
	%node1Name = "node" @ %node1;
	%node2Name = "node" @ %node2;
	%node1.setName(%node1Name);
	%node2.setName(%node2Name);
	%node1.nextNode = %node2Name;
	%node2.nextNode = %node1Name;
	%obj.moveOnPath(%node1);
	//mountTo(%node2, %marb);
	schedule(%time*2, 0, "eval", %obj @ ".cancelMoving();" SPC %node1 @ ".delete();" SPC %node2 @ ".delete();");

}

//function mountTo(%obj1, %obj2) {
//	if(isObject(%obj1)) {
//		echo("moving" SPC %obj1);
//		%obj1.setTransform(%obj2.getPosition());
//		schedule(100, 0, "mountTo", %obj1, %obj2);
//	}
//}

function clientCbOnServerJoin() {
	activatePackage(tempGems);
	activatePackage(BumperFireball);
}

function clientCbOnServerLeave() {
	deactivatePackage(BumperFireball);
	deactivatePackage(fakePad);
	deactivatePackage(tempGems);
}