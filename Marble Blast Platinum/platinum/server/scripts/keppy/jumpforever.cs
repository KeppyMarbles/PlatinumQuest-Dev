function KeppyPlayground::loop(%this) {
	%this.startCounter();
	%this.clearJumps();
	%this.schedule(30000, "loop");
}

function KeppyPlayground::startCounter(%this) {
	if($Game::State $= "start") {
		cancel(PlayGui.countdownSchedule);
		PlayGui.countdownSchedule = PlayGui.schedule(4000, "startCountdown", 26000, "timerHuntRespawn");
	}
	else
		PlayGui.startCountdown(30000, "timerHuntRespawn");
}

// Reset if all gems are collected
// Get TTs back?
// Create floor gem interior?
// Important: this needs to be a multiplayer level.

function KeppyPlayground::generateJumps(%this) {
	%this.createSets("jump1", 5);
	%this.createSets("jump2", 2);
	%this.createSets("jump3", 5);
	%this.createSets("jump4", 3);
	//%this.createSets("floorGem", 5);
}

function KeppyPlayground::checkIntersections(%this, %box) {
	for(%i = 0; %i < PlaygroundGroup.getCount(); %i++) {
		%obj = PlaygroundGroup.getObject(%i);
		if(%obj.box $= "")
			continue;
		if(boxIntersectionTest(%box, %obj.box))
			return %obj;
	}
	return false;
}

function KeppyPlayground::createSets(%this, %type, %amount) {
	if($PlayingDemo) {
		echo("Todo");
	}
	else {
		%sets = 0;
		%checks = 0;
		while(%sets != %amount) {
			%checks++;
			if(%checks > 100) {
				echo("Checks exceeded maximum");
				break;
			}
			%id = getRandom(1, 4);
			%xPos = getRandom(0, 50);
			%yPos = getRandom(0, 60);
			%rot = 90 * getRandom(0, 3);
		
			%part = new InteriorInstance() {
				position = %xPos SPC %yPos SPC "0";
				rotation = "0 0 1" SPC %rot;
				interiorFile = "platinum/data/interiors_mbg/custom/jump/" @ %type @ ".dif";
				temporary = true;
			};
			%this.set[%this.jumps] = %part;
		
			%box = %part.getWorldBox();
			%part.box = %box;
		
			if(%this.checkIntersections(%box)) {
				%part.delete();
				continue;
			}
		
			PlaygroundGroup.add(%part);
			%part.box = %box;
			%sets++;
			
			%this.jumps++;
		
			%node[2] = new StaticShape() {
				position = %part.position;
				rotation = %part.rotation;
				dataBlock = "PathNode";
					Smooth = "1";
					placed = "1";
					usePosition = "1";
					useRotation = "1";
					temporary = true;
			};
			%node[1] = new StaticShape() {
				position = vectorAdd(%part.position, "0 0 -10");
				rotation = %part.rotation;
				dataBlock = "PathNode";
				delay = %this.jumps * 50;
					Smooth = "1";
					nextNode = %node[2];
					placed = "1";
					timeToNext = "500";
					usePosition = "1";
					useRotation = "1";
					temporary = true;
			};
			
			%part.moveOnPath(%node[1]);
			%time = %node[1].delay+%node[1].timeToNext;
			%part.schedule(%time+300, "magicButton");
			%part.schedule(%time+100, "cancelMoving");
			%node[1].schedule(%time+200, 0, "delete");
			%node[2].schedule(%time+200, 0, "delete");
		}
	}
}

function SimObject::noteBox(%this) {
	%this.box = %this.getWorldBox();
}

function SimGroup::recurseCall(%this, %func, %a1) {
	for(%i = 0; %i < %this.getCount(); %i++) {
		%obj = %this.getObject(%i);
		if(%obj.getClassName() $= "SimGroup")
			%obj.recurseCall(%func, %a1);
		else
			%obj.call (%func, %a1);
	}
}

function SimObject::removeItem(%this) {
	if(%this.getClassName() $= "Item")
		%this.onNextFrame("delete");
}

function SimObject::removeTemp(%this) {
	if(%this.temporary)
		%this.onNextFrame("delete");
}

function KeppyPlayground::init(%this) {
	MissionGroup.recurseCall("removeTemp");
	PlaygroundGroup.forEach("%this.removeItem");
	%this.delete();
	MissionGroup.add(new ScriptObject(Playground) {
		class = "KeppyPlayground";
		jumps = 0;
	});
}

function KeppyPlayground::clearJumps(%this) {
	%this.schedule(%this.jumps * 50 + 700, "generateJumps");
	PlaygroundGroup.forEach("%this.removeItem");
	for(%parts = %this.jumps-1; %parts >= 0; %parts--) {
		%part = %this.set[%parts];

		%node[2] = new StaticShape() {
			position = vectorAdd(%part.position, "0 0 -10");
			rotation = %part.rotation;
			dataBlock = "PathNode";
				Smooth = "1";
				placed = "1";
				usePosition = "1";
				useRotation = "1";
				temporary = true;
		};
		%node[1] = new StaticShape() {
			position = %part.position;
			rotation = %part.rotation;
			dataBlock = "PathNode";
			delay = %parts * 50;
				Smooth = "1";
				nextNode = %node[2];
				placed = "1";
				timeToNext = "500";
				usePosition = "1";
				useRotation = "1";
				temporary = true;
		};
		
		%part.moveOnPath(%node[1]);
		%time = %node[1].delay+%node[1].timeToNext;
		%part.schedule(%time+100, "delete");
		%node[1].schedule(%time+100, "delete");
		%node[2].schedule(%time+100, "delete");
	}
}

function clientCbOnRespawn() {
	Playground.init();
	Playground.loop();
}

function clientCbOnMissionEnded() {
	cancel(PlayGui.countdownSchedule);
}
