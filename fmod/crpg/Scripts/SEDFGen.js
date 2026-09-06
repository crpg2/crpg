var eventsList = new Array()
var parametersList = new Array()

var exportingEventsList = new Array()
var exportingParameterList = new Array()
var exportingBanksList = new Array()

var SEDFExportBankBuildDir = studio.project.filePath + "\\..\\ExampleSoundMod\\"
var SEDFExportTrailingDir = "\\ModuleData\\"
var alwaysPlayMaxDistance = 100000

studio.menu.addMenuItem({
	name: "Taleworlds\\Bank Build and SEDF Gen",

	execute: function () {
		bankBuilds()
		getAllEventsAndParameters()
		parseEvents()
		parseParameters()
		exportXMLs(exportingBanksList)

		exportingEventsList = []
		exportingParameterList = []
		exportingBanksList = []
		eventsList = []
		parametersList = []
		studio.ui.showModalDialog({
			windowTitle: "SEDF Script",
			windowWidth: 300,
			windowHeight: 150,
			widgetType: studio.ui.widgetType.Layout,
			layout: studio.ui.layoutType.VBoxLayout,
			spacing: 10,
			items: [
				{
					widgetType: studio.ui.widgetType.Label,
					text: "Finished SEDF Generation!"
				},
				{
					widgetType: studio.ui.widgetType.PushButton,
					text: "OK",
					onClicked: function () {
						this.closeDialog();
					}
				}
			]
		})
	}
})

function bankBuilds() {
	bankList = studio.project.model.Bank.findInstances()
	
	bankList.sort(function(a, b) {
		if (a.note === undefined) {
			if(b.note === undefined) {
				return 0
			}
			return 1
		}
		return a.note.localeCompare(b.note)
	} )

	bankList.reverse()

	getAllEventsAndParameters()
	parseEvents()
	parseParameters()

	var projectDir = studio.project.workspace.builtBanksOutputDirectory
	studio.project.workspace.builtBanksOutputDirectory = SEDFExportBankBuildDir + "\\Sounds\\"

	bankList.forEach(function(bank) {
		if(bank.isMasterBank)
		{
			exportingBanksList.push({ name: bank.name + ".strings", id : bank.id })
		}
		else
		{
			exportingBanksList.push({ name: bank.name, id : bank.id })
		}
		studio.project.build({ banks: bank.name, platforms : 'Desktop'})

	})
	
	exportXMLs(exportingBanksList)
	studio.project.workspace.builtBanksOutputDirectory = projectDir
	eventsList = []
	parametersList = []
	
}

// Get all events, parameters and sort them
function getAllEventsAndParameters() {
	eventsList = studio.project.model.Event.findInstances()
	eventsList.sort(function (a, b) { return a.getPath().localeCompare(b.getPath()) })

	getAllParametersOfEventList()
	parametersList = parametersList.filter( function(a) { return a.parameter.parameterType < 2 || !(a.parameter.enumerationLabels == null)}) // filter game controlled parameters
	parametersList.sort(function (a, b) { return a.getPath().localeCompare(b.getPath()) })
}

function getAllParametersOfEventList() {
	eventsList.forEach(function(event) {
		var parameterPresets = event.getParameterPresets()
		parameterPresets.forEach( function(param) {
			if(parametersList.find( function(a) { return a.name == param.presetOwner.name } ) == undefined)
			{
				parametersList.push(param.presetOwner)
			}
		})
	})
}

function parseParameters() {
	if (parametersList.length > 0) {
		parametersList.forEach(function (parameter) {
			//guid, name
			exportingParameterList.push({ guid: parameter.id, name: parameter.name })
		})
	}
}

//length, max_distance, priority_multiplier, bassgid, parameter_indices, send_user_param, send_occlusion
function parseEvents() {
	if (eventsList.length > 0) {
		eventsList.forEach(function (targetEvent) {
			var do_not_include_in_xml = false
			targetEvent.relationships.folder.destinations.forEach(function (dest) {
				if (dest.entity == "Event") {
					do_not_include_in_xml = true
				}
			})

			if (targetEvent.banks.length === 0) {
				do_not_include_in_xml = true
			}
			if (!do_not_include_in_xml) {
				var guid, path, length_time = -1, max_distance = alwaysPlayMaxDistance, priority_multiplier = 1, bassgid = -1, parameter_indices = [], send_user_param = false, send_occlusion = false
				var ignore_max_distance = 0

				// Get GUID
				guid = targetEvent.id
				path = targetEvent.getPath()

				// Get Length
				if (targetEvent.isOneShot()) {
					//TODO Check if timeline exists as well - support Action sheets
					//Temporarily added UserProperty override
					targetEvent.userProperties.forEach(function (userProperty) {
						if (userProperty.key == "maxLength") {
							length_time = userProperty.value
						}
					})

					targetEvent.timeline.modules.forEach(function (module) {
						current_end_time = module.length + module.start
						length_time = current_end_time > length_time ? current_end_time : length_time
					})

					if (length_time <= 0) {
						alert("Invalid event duration!\n\nPlease provide either a Timeline or a 'maxLength' user property.\n\n" + targetEvent.getPath())
					}
				}

				// Get user properties, priority and bassgid
				targetEvent.userProperties.forEach(function (userProperty) {
					switch (userProperty.name) {
						case "priorityMultiplier":
							priority_multiplier = userProperty.value
							break
						case "battle_ambient_group_id":
							bassgid = BASSGIDS[userProperty.value]
							break
						case "ignoreMaxDistance":
							ignore_max_distance = userProperty.value
							break
						default:
							break
					}
				})
				
				// Get Max Distance
				if (targetEvent.is3D() && !ignore_max_distance) {
					max_distance = targetEvent.automatableProperties.maximumDistance
				}
				else {
					max_distance = alwaysPlayMaxDistance
				}
					

				// Get parameter indices
				targetEvent.getParameterPresets().forEach(function (event_parameter) {
					if (event_parameter.parameterType < 2 || !(event_parameter.enumerationLabels == null)) {
						parameter_indices.push(event_parameter.presetOwner.name)
					}
				})

				// Occlusion and isPlayer
				targetEvent.getParameterPresets().forEach(function (parameter) {
					if (parameter.presetOwner.name == "Occlusion") {
						send_occlusion = true
					}
					if (parameter.presetOwner.name == "isPlayer") {
						send_user_param = true
					}
				})

				exportingEventsList.push({ guid: guid, path: path, length_time: length_time, max_distance: max_distance, priority_multiplier: priority_multiplier, bassgid: bassgid, parameter_indices: parameter_indices, send_user_param: send_user_param, send_occlusion: send_occlusion })
			}
		})
	}
}

function exportXMLs(exportingBanksList) {
	var eventsFile = studio.system.getFile(SEDFExportBankBuildDir + SEDFExportTrailingDir +"sound_event_data.gen.xml")
	var parametersFile = studio.system.getFile(SEDFExportBankBuildDir + SEDFExportTrailingDir +"sound_params.gen.xml")
	var soundfilesFile = studio.system.getFile(SEDFExportBankBuildDir + SEDFExportTrailingDir +"soundfiles.xml")
	
	
	eventsFile.remove()
	eventsFile.close()

	parametersFile.remove()
	parametersFile.close()

	soundfilesFile.remove()
	soundfilesFile.close()

	var eventsFile = studio.system.getFile(SEDFExportBankBuildDir  +"sound_event_data.gen.xml")
	var parametersFile = studio.system.getFile(SEDFExportBankBuildDir  +"sound_params.gen.xml")
	var soundfilesFile = studio.system.getFile(SEDFExportBankBuildDir  +"soundfiles.xml")
	eventsFile.open(studio.system.openMode.WriteOnly)

	//Heading
	eventsFile.writeText("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<base>")
	eventsFile.writeText("\n<events>")

	//Events
	exportingEventsList.forEach(function (event) {
		//parameter indice formatting
		var indices
		if (event.parameter_indices.length <= 0) {
			indices = ""
		} else {
			indices = event.parameter_indices.join(",")
		}

		eventsFile.writeText("\n\t<event")
		eventsFile.writeText("\n\t\tguid=\"" + event.guid + "\"")
		eventsFile.writeText("\n\t\tpath=\"" + event.path + "\"")
		eventsFile.writeText("\n\t\tlength=\"" + event.length_time + "\"")
		eventsFile.writeText("\n\t\tmax_distance=\"" + event.max_distance + "\"")
		eventsFile.writeText("\n\t\tpriority_multiplier=\"" + event.priority_multiplier + "\"")
		eventsFile.writeText("\n\t\tbassgid=\"" + event.bassgid + "\"")
		eventsFile.writeText("\n\t\tparameter_indices=\"" + indices + "\"")
		eventsFile.writeText("\n\t\tsend_is_player=\"" + event.send_user_param + "\"")
		eventsFile.writeText("\n\t\tsend_occlusion=\"" + event.send_occlusion + "\"/>")
	})

	//Ending
	eventsFile.writeText("\n</events>")
	eventsFile.writeText("\n</base>")
	eventsFile.copy(SEDFExportBankBuildDir + SEDFExportTrailingDir + "sound_event_data.gen.xml")
	eventsFile.remove()
	eventsFile.close()

	
	parametersFile.open(studio.system.openMode.WriteOnly)
	//Heading
	parametersFile.writeText("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<base>")

	//Parameters
	exportingParameterList.forEach(function (parameter) {
		parametersFile.writeText("\n\t<parameter")
		parametersFile.writeText("\n\t\tguid=\"" + parameter.guid + "\"")
		parametersFile.writeText("\n\t\tname=\"" + parameter.name + "\"/>")
	})


	//Ending
	parametersFile.writeText("\n</base>")
	parametersFile.copy(SEDFExportBankBuildDir + SEDFExportTrailingDir + "sound_params.gen.xml")
	parametersFile.remove()
	parametersFile.close()


	
	soundfilesFile.open(studio.system.openMode.WriteOnly)
	//Heading
	soundfilesFile.writeText("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<base xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"\n\ttype=\"sound\">\n\t<asset_files>")

	//Soundfiles
	exportingBanksList.forEach(function (bank) {
		soundfilesFile.writeText("\n\t\t<file")
		soundfilesFile.writeText("\n\t\t\tname=\"" + bank.name + ".bank\"")
		soundfilesFile.writeText("\n\t\t\tdecompress_samples=\"false\"/>")
	})


	//Ending
	soundfilesFile.writeText("\n\t</asset_files>\n</base>")
	soundfilesFile.copy(SEDFExportBankBuildDir + SEDFExportTrailingDir + "soundfiles.xml")
	soundfilesFile.remove()
	soundfilesFile.close()


	//Flush lists to prevent appending
	exportingEventsList = []
	exportingParameterList = []
	exportingBanksList = []
}