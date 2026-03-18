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


// create static shapes (all in one file?) to reveal from a long distance before creating at a short distance

datablock AudioProfile(snapSfx1) {
	filename = "~/data/sound/custom/keppySnap1.wav";
	description = AudioDefault3d;
	preload = true;
};
datablock AudioProfile(snapSfx2) {
	filename = "~/data/sound/custom/keppySnap2.wav";
	description = AudioDefault3d;
	preload = true;
};
datablock AudioProfile(snapSfx3) {
	filename = "~/data/sound/custom/keppySnap3.wav";
	description = AudioDefault3d;
	preload = true;
};
datablock AudioProfile(snapSfx4) {
	filename = "~/data/sound/custom/keppySnap4.wav";
	description = AudioDefault3d;
	preload = true;
};

datablock StaticShapeData(MatrixSquare) {
	shapeFile = "~/data/shapes/custom/matrix/square.dts";
};

datablock StaticShapeData(MatrixRamp1) {
	shapeFile = "~/data/shapes/custom/matrix/ramp1.dts";
};

datablock StaticShapeData(MatrixRamp2) {
	shapeFile = "~/data/shapes/custom/matrix/ramp2.dts";
};

datablock StaticShapeData(MatrixRamp3) {
	shapeFile = "~/data/shapes/custom/matrix/ramp3.dts";
};

datablock StaticShapeData(MatrixRamp4) {
	shapeFile = "~/data/shapes/custom/matrix/ramp4.dts";
};

function opposeDir(%dir) {
	return %dir < 3 ? %dir + 2 : %dir - 2;
}

function debug(%str) {
	if($DEBUG)
		echo(%str);
}

function KeppyRamps::remove(%this, %obj) {
	debug("remove: Removing" SPC %obj);
	if(isObject(%obj.prev)) {
		%obj.prev.delete();
		debug("remove: Removed prev");
	}
	
	if(%obj.connectors) {
		%obj.connectors.forEach("%this.uncouple");
		%obj.connectors.delete();
		debug("remove: Removed connectors");
	}
		
	if(isObject(%obj.item)) {
		%obj.item.delete();
		debug("remove: Removed item");
	}
		
	
	if(isObject(%obj.endPad)) {
		%this.endPad = false;
		%obj.endPad.delete();
		debug("remove: Removed end pad");
	}
	
	%obj.delete();
	debug("remove: Removed" SPC %obj);
}

function KeppyRamps::CreatePlatform(%this, %pos, %id, %zdir, %pcon) {
	if(%id) {
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
		
		%connectors = new SimGroup();
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
		%connectors = new SimGroup();
		for(%i = 1; %i <= 4; %i++) {
			%connectors.add(new ScriptObject() {
				class = "RampConnector";
				pos = vectorAdd(%pos, MissionList.squarePos[%i]);
				next[1] = vectorAdd(%pos, MissionList.squareFront[%i]);
				next[-1] = vectorAdd(%pos, MissionList.squareBack[%i]);
				dir = %i;
			});
		}
		for(%_ = 0; %_ < getRandom(0, 3); %_++) {
			%connectors.getObject(getRandom(0, 3)).skip = 1;
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
	debug("CreatePlatform: Created platform" SPC %platform);
	%platformPrev = new StaticShape() {
		position = %pos;
		platform = %platform;
		dataBlock = %dataBlock;
	};
	%platformPrev.setFadeVal(0.5);
	debug("CreatePlatform: Created preview" SPC %platformPrev);
	%platform.prev = %platformPrev;
	
	for(%i = 0; %i < %connectors.getCount(); %i++) {
		%connector = %connectors.getObject(%i);
		%connector.platform = %platform;
	}
	
	%connectors.forEach("%this.couple", %this);
	
	debug("CreatePlatform: Adding platform objects to their groups");
	PreviewGroup.add(%platformPrev);
	PlatformGroup.add(%platform);
	ConnectorGroup.add(%connectors);
	debug("CreatePlatform: Added" SPC %platformPrev SPC %platform SPC %connectors);
	%this.createItem(%platform);
}

function RampConnector::next(%this) {
	//debug("next: checking" SPC %this);
	if(%this.coupled || %this.skip)
		return;
	
	//debug("next: Getting next gen for connector" SPC %this);

	%pos = %this.next[1];

	if(%this.platform.id)
		RampMatrix.CreatePlatform(%pos, 0, 0, %this);
	
	else {
		%id = %this.dir;
		%zdir = "up";
		
		if(getRandom(0, 1)) {
			%id = opposeDir(%id);
			%pos = %this.next[-1];
			%zdir = "down";
		}
		//if(!getRandom(0, 1))
			%this.CreateSquare(%pos);
		//else
		//	RampMatrix.CreatePlatform(%pos, %id, %zdir);
	}
}

function RampConnector::uncouple(%this) {
	%this.coupled.coupled = false;
	RampMatrix.connector[%this.pos] = %this.coupled;
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

function KeppyRamps::next(%this, %platform) {
	//debug("next:" SPC %platform);
	%platform.connectors.forEach("%this.next");
}

function TempEmitter(%pos, %type, %life) {
	debug("TempEmitter: Creating temp emitter");
	%emitter = new ParticleEmitterNode() {
		position = %pos;
		dataBlock = fireWorkNode;
		emitter = %type;
	};
	%emitter.schedule(%life, "delete");
}

function KeppyRamps::platformEffect(%this, %pos) {
	ServerPlay3D("bounce" @ getRandom(1, 4) @ "Sfx", %pos);
	TempEmitter(%pos, "LandMineSparkEmitter", 200);
}

function KeppyRamps::CreateItem(%this, %square) {
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

function KeppyRamps::EndPad(%this, %square) {
	debug("EndPad: Creating end pad");
	%this.endPad = true;
	ItemGroup.add(%pad = new StaticShape() {
		 position = vectorAdd(%square.position, "0 0 0.5");
		 rotation = "0 0 1 179.518";
		 dataBlock = "EndPad_MBG";
	});
	%square.endPad = %pad;
}

function KeppyRamps::StartPlatforms(%this) {
	$KeppyRamps::Create = true;
	debug("--------Platform creation started--------");
}

function KeppyRamps::StartRun(%this) {
	debug("StartRun: starting run");
	%this.delete();
	%this = new ScriptObject(RampMatrix) {
		class = "KeppyRamps";
	};
	MissionGroup.add(%this);
	$KeppyRamps::Create = false;
	debug("StartRun: Clearing ramp objects");
	MatrixGroup.forEach("%this.clear");
	debug("Finished clearing objects");
	
	%this.schedule(500, "createPlatform", "0 0 0", 0);
	
	
	CommandToClient(LocalClientConnection, 'SetGemQuota', 20, 10);
	ClientMode_quota.shouldUpdateGems();
	%this.schedule(2000, "StartPlatforms");
}

function clientCbOnRespawn() {
	RampMatrix.StartRun();
}

function KeppyRamps::GetRampZDir(%this, %ramp, %obj) {
	if(%ramp $= "")
		return;
	if(getWord(%obj.position, 2) > getWord(%ramp.position, 2))
		return "down";
	else
		return "up";
}

function KeppyRamps::BuildTrim(%this, %obj) {
	debug("BuildTrim: Building trim for" SPC %obj);
	if(%obj.id) {
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
			%firstConnector = %obj.connectors.getObject(%i);
			%next = %next == 3 ? 0 : %i+1;
			%secondConnector = %obj.connectors.getObject(%next);
			
			%rot = "0 0 1" SPC 90 * %i;
			
			
			if(%firstConnector.skip && !%firstConnector.coupled) {
				%trim = new InteriorInstance() {
					position = %obj.position;
					rotation = %rot;
					interiorFile = "platinum/data/interiors_mbg/matrix/trim.dif";
				};
				debug("BuildTrim: Created edge trim" SPC %trim);
				%firstConnector.trim = %trim;
				TrimGroup.add(%trim);
			}
			
			%firstDir = %this.GetRampZDir(%firstConnector.coupled.platform, %obj);
			%secondDir = %this.GetRampZDir(%secondConnector.coupled.platform, %obj);
			
			%trim = new InteriorInstance() {
				position = %obj.position;
				rotation = %rot;
				interiorFile = "platinum/data/interiors_mbg/matrix/trim_" @ %firstDir @ "_" @ %secondDir @ ".dif";
			};
			debug("BuildTrim: Created corner trim" SPC %trim);
			TrimGroup.add(%trim);
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

function KeppyRamps::AddObject(%this, %obj) {
	debug("AddObject: Adding" SPC %obj);
	if(!isObject(%obj)) {
		debug("AddObject: Object was removed");
		return;
	}
	%obj.setScale("1 1 1");
	%this.platformEffect(%obj.position);
	if(!%obj.id)
		%this.squares++;

	//if(%obj.explode) {
	//	ServerPlay3D(ExplodeMineSfx, %obj.explode.getWorldBoxCenter());
	//	%this.remove(%obj.explode, 1);
	//}
	%this.BuildTrim(%obj);

	if(%obj.item)
		%obj.item.setFadeVal(1);
	
	if(%obj.endPad) {
		LocalClientConnection.player.setPad(%obj.endPad);
	}
	
	if(%obj.gem)
		%this.gems++;
	
	%obj.prev.delete();
	%obj.prev = "";
	debug("AddObject: Added object" SPC %obj);
}

function InteriorInstance::RampsOnFrameAdvance(%this) {
	if(!$KeppyRamps::Create) {
		debug("Frame Advance: Create var is false!");
	}
		
	//%dist = VectorDist($MarblePos, %this.position);
	%sub = VectorSub(%this.position, $MarblePos);
	%dist = VectorDot(%sub, %sub);
	
	debug("----");
	debug(PlatformGroup.getCount());
	debug(%this);
	debug(%this.position);
	debug($MarblePos);
	debug(%dist);
	
	if(%dist < 256) {
		debug("Calling next");
		//if(RampMatrix.gems < 20)
			RampMatrix.next(%this);
	}
		
	
	if(%this.prev) {
		debug("Has prev");
		
		if(%dist < 64) {
			debug("Calling add object");
			RampMatrix.AddObject(%this);
		}
			
		
		if(%dist > 576) {
			debug("Calling remove object");
			//if(RampMatrix.gems < 20)
				RampMatrix.remove(%this);
		}
			
	}
	debug("End interior frame advance");
}

function clientCbOnFrameAdvance() {
	debug("advancing frame");
	if(!$PlayingDemo && !$Game::Menu && $KeppyRamps::Create) {
		debug("------------------");
		$MarblePos = $MP::MyMarble.getPosition();
		PlatformGroup.forEach("%this.RampsOnFrameAdvance");
		debug("Finished group call");
	}
		
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
				break;
			case 1:
				%obj = new Item() {
					dataBlock = %data;
					rotate = 1;
					static = 1;
					collideable = 0;
				};
				break;
			case 2:
				%obj = new StaticShape() {
					dataBlock = %data;
				};
				break;
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