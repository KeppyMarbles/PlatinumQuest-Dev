// Procedural
// Each interior type has an array of slots positions relative to its pos
// An added interior must have an slots position that matches the location of its parent
// An added interior cannot intersect with any other

// Or
// Each interior has a list of possible configurations with another interior

// slots slots must connect to air or another slots slot

// Ramp makes 1 to 3 children, trim made on slots

// Set transform for gems and end pad
// don't use random/brute force to find vaild paths
// stop producing (and build trim) when all gems are created
// Remove trim if there's an intersection

// use connection points instead of slots?
// squares have 4, ramps have 2

// remove (explode) invalid platforms each addition


// time travels?

// Lower platforms: can spawn a super jump
// Higher platforms: can spawn a gyrocopter/shock absorber
// Middle: super speed

// Maybe add collision to the preview shapes so that you can't get stuck inside the interior
// Need to increase backtracking. Maybe we could build dead ends if there are branches available.

datablock StaticShapeData(MatrixSquare) { shapeFile = "~/data/shapes/custom/matrix/square.dts"; };
datablock StaticShapeData(MatrixRamp1) { shapeFile = "~/data/shapes/custom/matrix/ramp1.dts"; };
datablock StaticShapeData(MatrixRamp2) { shapeFile = "~/data/shapes/custom/matrix/ramp2.dts"; };
datablock StaticShapeData(MatrixRamp3) { shapeFile = "~/data/shapes/custom/matrix/ramp3.dts"; };
datablock StaticShapeData(MatrixRamp4) { shapeFile = "~/data/shapes/custom/matrix/ramp4.dts"; };

function KeppyRamps::onMissionReset(%this) {
	debug("StartRun: starting run");
	LocalClientConnection.setPad(false);
	%this.delete();
	%this = new ScriptObject(RampMatrix) {
		class = "KeppyRamps";
	};
	MissionGroup.add(%this);

	MatrixGroup.forEach("%this.clear");
	
	%this.schedule(500, "createPlatform", "0 0 0", 0);
	
	CommandToClient(LocalClientConnection, 'SetGemQuota', 20, 10);
	ClientMode_quota.shouldUpdateGems();
	%this.schedule(2000, "tick");
}

function KeppyRamps::tick(%this) {
	if($PlayingDemo || $Game::Menu)
		return;

	%marblePos = $MP::MyMarble.getPosition();
	%zScale = 3;
	for(%i = 0; (%obj = PlatformGroup.getObject(%i)) != -1; %i++) { // TODO maybe use distance to connector?
		%sub = VectorSub(%obj.position, %marblePos);
		%sub = setWord(%sub, 2, getWord(%sub, 2) * %zScale);
		%sqDist = VectorDot(%sub, %sub);
		
		if(%sqDist < 512) {
			
			//if(RampMatrix.gems < 20)
				%obj.connectors.forEach("%this.next");
		}
		
		if(%obj.prev) {
			if(%sqDist < 128) {
				%this.addPlatform(%obj);
			}
			
			if(%sqDist > 1024) {
				//if(RampMatrix.gems < 20)
					%this.removePlatform(%obj);
			}
		}
	}

	cancel(%this.tickSch);
	%this.tickSch = %this.schedule(100, "tick");
}

function RampConnector::next(%this) {
	 // TODO don't build outside a certain bounds?
	 // TODO don't build if there's already a bunch of gems generated?
	 // TODO maybe increase the chance of dead ends with higher platform count?
	if(%this.coupled || %this.skip)
		return;

	%pos = %this.next[1];

	if(%this.platform.id > 0) // Create square to connect with ramp
		RampMatrix.createPlatform(%pos, 0, 0, %this);
	
	else { // Create ramp to connect with square
		%id = %this.dir;
		%zdir = "up";
		
		if(getRandom(0, 1)) {
			%id = opposeDir(%id);
			%pos = %this.next[-1];
			%zdir = "down";
		}

		RampMatrix.createPlatform(%pos, %id, %zdir, %this);
	}
}

function KeppyRamps::createPlatform(%this, %pos, %id, %zdir, %pcon) {
	%connectors = new SimGroup();

	if(%id > 0) {
		debug("CreatePlatform: Creating ramp");
		%dir = mAbs(%id);
		
		%pos[0] = vectorAdd(%pos, MissionList.backPos[%id]);
		%pos[1] = vectorAdd(%pos, MissionList.frontPos[%id]);
		%next[0] = vectorAdd(%pos, MissionList.backNext[%id]);
		%next[1] = vectorAdd(%pos, MissionList.frontNext[%id]);
		%dir[0] = opposeDir(%dir);
		%dir[1] = %dir;
		
		for(%i = 0; %i < 2; %i++) {
			%ncon = %this.connector[%pos[%i]];
			if(%ncon.dir == %dir[%i] || %ncon.coupled) {
				%pcon.skip = true;
				return;
			}
		}
		
		for(%i = 0; %i < 2; %i++) {
			%connectors.add(new ScriptObject() {
				class = "RampConnector";
				pos = %pos[%i];
				next[1] = %next[%i]; 
				dir = %dir[%i];
			});
		}

		%interiorFile = "platinum/data/interiors_mbg/matrix/ramp" @ %id @ ".dif";
		%dataBlock = "MatrixRamp" @ %id;
	}
	else {
		debug("CreatePlatform: Creating square");
		for(%i = 1; %i <= 4; %i++) {
			%connectors.add(new ScriptObject() {
				class = "RampConnector";
				pos = vectorAdd(%pos, MissionList.squarePos[%i]);
				next[1] = vectorAdd(%pos, MissionList.squareFront[%i]);
				next[-1] = vectorAdd(%pos, MissionList.squareBack[%i]);
				dir = %i;
			});
		}
		for(%_ = 0; %_ < 4; %_++) {
			%connectors.getObject(getRandom(0, 4)).skip = 1;
		}
		%interiorFile = "platinum/data/interiors_mbg/matrix/square.dif";
		%dataBlock = "MatrixSquare";
	}
	
	%platform = new InteriorInstance() {
		scale = "0 0 0";
		position = %pos;
		connectors = %connectors;
		interiorFile = %interiorFile;
		zdir = %zdir;
		id = %id;
		num = %this.squares;
	};

	%platformPrev = new StaticShape() {
		position = %pos;
		platform = %platform;
		dataBlock = %dataBlock;
	};
	%platformPrev.setFadeVal(0.5);

	%platform.prev = %platformPrev;
	
	for(%i = 0; (%conn = %connectors.getObject(%i)) != -1; %i++) {
		%conn.platform = %platform;
	}
	
	%connectors.forEach("%this.couple", %this);
	
	debug("CreatePlatform: Adding platform objects to their groups");
	PreviewGroup.add(%platformPrev);
	PlatformGroup.add(%platform);
	ConnectorGroup.add(%connectors);
	debug("CreatePlatform: Added" SPC %platformPrev SPC %platform SPC %connectors);
	%this.createItem(%platform);
}

function KeppyRamps::buildTrim(%this, %obj) {
	debug("BuildTrim: Building trim for" SPC %obj);
	if(%obj.prev)
		return;
	if(%obj.id > 0) {
		%trim = new InteriorInstance() {
			position = %obj.position;
			rotation = "0 0 1" SPC 90 * (%obj.id-1);
			interiorFile = "platinum/data/interiors_mbg/matrix/trim_ramp.dif";
		};
		debug("BuildTrim: Created ramp trim" SPC %trim);
		TrimGroup.add(%trim);
	}
	else {
		for(%i = 0; %i < 4; %i++) {
			%next = (%i + 1) % 4; 
			%firstConnector = %obj.connectors.getObject(%i);
			%secondConnector = %obj.connectors.getObject(%next);

			%rot = "0 0 1" SPC 90 * %i;
			
			// Handle Edge Trim (Only build if it doesn't already exist)
			if(%firstConnector.skip && !%firstConnector.coupled) {
				if (!isObject(%firstConnector.trim)) {
					%trim = new InteriorInstance() {
						position = %obj.position;
						rotation = %rot;
						interiorFile = "platinum/data/interiors_mbg/matrix/trim.dif";
					};
					debug("BuildTrim: Created edge trim" SPC %trim);
					%firstConnector.trim = %trim;
					TrimGroup.add(%trim);
				}
			}
			
			// Remove existing trim
			if (isObject(%obj.cornerTrim[%i])) {
				%obj.cornerTrim[%i].delete();
			}

			%firstPlat = %firstConnector.coupled.platform;
			%secondPlat = %secondConnector.coupled.platform;

			%corner1 = (isObject(%firstPlat) && !isObject(%firstPlat.prev)) || %firstConnector.skip;
			%corner2 = (isObject(%secondPlat) && !isObject(%secondPlat.prev)) || %secondConnector.skip;

			// ONLY build corner if both adjacent platforms exist and are NOT previews
			//if (isObject(%firstPlat) && !isObject(%firstPlat.prev) &&
			//    isObject(%secondPlat) && !isObject(%secondPlat.prev)) {
			if(%corner1 && %corner2) {
				%firstDir = %this.getRampZDir(%firstPlat, %obj);
				%secondDir = %this.getRampZDir(%secondPlat, %obj);
				
				%trim = new InteriorInstance() {
					position = %obj.position;
					rotation = %rot;
					interiorFile = "platinum/data/interiors_mbg/matrix/trim_" @ %firstDir @ "_" @ %secondDir @ ".dif";
				};
				debug("BuildTrim: Created corner trim" SPC %trim);
				
				%obj.cornerTrim[%i] = %trim; 
				TrimGroup.add(%trim);
			}
		}
	}
	for(%i = 0; %i < %obj.connectors.getCount(); %i++) {
		%connector = %obj.connectors.getObject(%i);
		if(%connector.coupled.trim) {
			debug("BuildTrim: Deleting some trim");
			//ServerPlay3D(ExplodeMineSfx, %connector.coupled.trim.getWorldBoxCenter());
			%connector.coupled.trim.delete();
			
			%connector.coupled.trim = "";
		}
	}
	debug("BuildTrim: Built trim");
}

function KeppyRamps::addPlatform(%this, %obj) {
	debug("AddObject: Adding" SPC %obj);
	if(!isObject(%obj)) {
		debug("AddObject: Object was removed");
		return;
	}
	%obj.setScale("1 1 1");
	%this.platformEffect(%obj.position);

	//if(%obj.explode) {
	//	ServerPlay3D(ExplodeMineSfx, %obj.explode.getWorldBoxCenter());
	//	%this.remove(%obj.explode, 1);
	//}


	if(%obj.item)
		%obj.item.setFadeVal(1);
	
	if(%obj.endPad)
		LocalClientConnection.player.setPad(%obj.endPad);
	
	if(%obj.gem) {
		%this.gems++;
		echo("gems:" SPC %this.gems);
	}
		
	
	%obj.prev.delete();
	%obj.prev = "";

	%this.buildTrim(%obj);

	if (%obj.id > 0) { // Rebuild connected square trim if this is a ramp
		for(%i = 0; (%conn = %obj.connectors.getObject(%i)) != -1; %i++) {
			if (isObject(%conn.coupled) && %conn.coupled.platform.id == 0) {
				%this.buildTrim(%conn.coupled.platform);
			}
		}
	}
	else
		%this.squares++;

	debug("AddObject: Added object" SPC %obj);
}

function KeppyRamps::getRampZDir(%this, %ramp, %obj) {
	if(%ramp $= "")
		return;
	if(getWord(%obj.position, 2) > getWord(%ramp.position, 2))
		return "down";
	else
		return "up";
}

function KeppyRamps::platformEffect(%this, %pos) {
	ServerPlay3D("bounce" @ getRandom(1, 4) @ "Sfx", %pos);
	spawnEmitter(200, LandMineSparkEmitter, %pos, false);
}

function KeppyRamps::removePlatform(%this, %obj) {
	if(isObject(%obj.prev)) {
		%obj.prev.delete();
	}
	
	if(%obj.connectors) {
		%obj.connectors.forEach("%this.uncouple");
		%obj.connectors.delete();
	}
		
	if(isObject(%obj.item)) {
		%obj.item.delete();
	}

	if(isObject(%obj.gem)) {
		%this.gems--;
		echo("gems:" SPC %this.gems);
	}
	
	if(isObject(%obj.endPad)) {
		%this.endPad = false;
		%obj.endPad.delete();
		LocalClientConnection.player.setPad(false);
	}
	
	%obj.delete();
	debug("remove: Removed" SPC %obj);
}

function KeppyRamps::createItem(%this, %square) {
	debug("Create item:" SPC %square);
	if(%square.id || %square.num == 0)
		return;
	
	if(%square.num % 2 == 0 && %this.gems < 20) {
		%item = "GemItem";
		%square.gem = true;
	}
	
	if(%square.num > 10 && !%this.EndPad) {
		%this.EndPad(%square);
	}
		
	else {
		%rand = getRandom(0, 15);

		if(%rand == 1)
			%item = "SuperJumpItem";
		else if(%rand == 2)
			%item = "HelicopterItem";
		else if (%rand == 3)
			%item = "SuperSpeedItem";
	}

	if(%item !$= "") {
		debug("CreateItem: Creating item" SPC %item);
		%square.item = new Item() {
			position = vectorAdd(%square.position, "0 0 0.75");
			dataBlock = %item;
			collideable = "0";
			static = "1";
			rotate = "1";
		};
		ItemGroup.add(%square.item);
		%square.item.setFadeVal(0.5);
	}
}

function KeppyRamps::endPad(%this, %square) { // TODO maybe just do TSStatic for prev?
	debug("EndPad: Creating end pad");
	%this.endPad = true;
	ItemGroup.add(%pad = new StaticShape() {
		position = vectorAdd(%square.position, "0 0 0.5");
		rotation = "0 0 1 179.518";
		dataBlock = "EndPad_MBG";
	});
	%square.endPad = %pad;
}

function RampConnector::couple(%this) {
	%diff_conn = RampMatrix.connector[%this.pos];
	if(%diff_conn){
		if(%diff_con != %this) {
			//if(%diff_conn.coupled) {
			//	%this.platform.explode = %diff_conn.coupled.platform;
			//}
			%diff_conn.coupled = %this;
			%this.coupled = %diff_conn;
		}
	}
	else {
		RampMatrix.connector[%this.pos] = %this;
	}
}

function RampConnector::uncouple(%this) {
	%this.coupled.coupled = false;
	RampMatrix.connector[%this.pos] = %this.coupled;
}

package RampMatrixRecord {
	function PlaybackInfo::readScores(%this) {
		// 
		%type = %this.fo.readRawS8();
		%data = %this.fo.readRawS8();
		%position = %this.fo.readRawString8();
		switch (%type) {
			case 0:
				%obj = new InteriorInstance() {
					position = %position;
					interiorFile = %data;
				};
			case 1:
				%obj = new Item() {
					dataBlock = %data;
					rotate = 1;
					static = 1;
					collideable = 0;
				};
			case 2:
				%obj = new StaticShape() {
					dataBlock = %data;
				};
		}
		MissionGroup.add(%obj);
	}
	
	function recordWriteScores(%stream, %type, %data, %position) {
		Parent::recordWriteScores(%stream);
		%stream.writeRawS8(%type);
		%stream.writeRawString8(%data);
		%stream.writeRawString8(%position);
	}
};

function opposeDir(%dir) {
	return %dir < 3 ? %dir + 2 : %dir - 2;
}

function debug(%str) {
	if($DEBUG)
		echo(%str);
}

//RampMatrix.TestBuild(100);
//RampMatrix.BuildTrim();
//RampMatrix.StartRun();


//function KeppyRamps::TestBuild(%this, %max) {
//	RampGroup.clear();
//	%this.max = %max;
//	%this.createSquare(-2, "0 0 0");
//	%this.max *= 2;
//	for(%i = 0; %i < RampGroup.getCount(); %i++) {
//		%obj = RampGroup.getObject(%i);
//		if(getWordCount(%obj.slots) > 0) {
//			debug(%obj SPC "needs another thing");
//			%this.SquareNext(%obj);
//		}
//	}
//	//%this.BuildTrim();
//}
//
//function KeppyRamps::BuildTrim(%this) {
//	for(%i = 0; %i < RampGroup.getCount(); %i++) {
//		%square = RampGroup.getObject(%i);
//		for(%j = 0; %j < getWordCount(%square.slots); %j++) {
//			%trim = new InteriorInstance() {
//				position = %square.position;
//				interiorFile = "platinum/data/interiors_mbg/matrix/trim" @ getWord(%square.slots, %j) @ ".dif";
//			};
//			RampGroup.add(%trim);
//			%square.slots = RemoveWord(%square.slots, %j);
//		}
//	}
//}