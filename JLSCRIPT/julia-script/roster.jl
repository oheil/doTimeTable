################################################################################
# Copyright 2019 Oliver Heil, heilbIT
# 
# This file is part of doTimeTable.
# 
# doTimeTable is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
# 
# doTimeTable is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
# GNU General Public License for more details.
# 
# You should have received a copy of the GNU General Public License
# along with doTimeTable.  If not, see <https://www.gnu.org/licenses/>.
# 
################################################################################

using DelimitedFiles
using Dates
using Random

if VERSION >= v"0.7.0-"
	currentStdOut=stdout
else
	currentStdOut=STDOUT
end

import Base.+,Base.-
+(::Nothing, ::Nothing) = nothing
+(::Nothing, n::Number) = n
+(n::Number, ::Nothing) = n
-(::Nothing, ::Nothing) = nothing
-(::Nothing, n::Number) = n
-(n::Number, ::Nothing) = n

progress=0
println("###PROGRESS###",progress+=1)

function prune(a::AbstractArray)
	a[a[:].==""].=0
	numberRows = size(a,1)
	emptyRows = trues(numberRows)
	for row in 1:numberRows
		if sum(a[row,:].!=0)==0
			emptyRows[row]=false
		end
	end
	numberCols = size(a,2)
	emptyCols = trues(numberCols)
	for col in 1:numberCols
		if sum(a[:,col].!=0)==0
			emptyCols[col]=false
		end
	end
	a[emptyRows,emptyCols]
end

small=""

relaxTeacherAvailability=false
maxRosterPrefillCount=1
backtraceWarningLimit=10000
classesInRandomOrder=0
coursesInOrderOfTeacherCoverage=false
if isfile("config"*small*".csv")
	println("reading: "*"config"*small*".csv")
	configFile=prune(readdlm("config"*small*".csv",';'))
	row=findfirst(isequal("relaxTeacherAvailability"),configFile[:,1])
	if ! isnothing(row)
		if configFile[row,2] != 0
			relaxTeacherAvailability = true
		end
	end
	row=findfirst(isequal("maxRosterPrefillCount"),configFile[:,1])
	if ! isnothing(row)
		if isinteger(configFile[row,2])
			maxRosterPrefillCount = configFile[row,2]
		end
		if maxRosterPrefillCount <= 0
			maxRosterPrefillCount = 1
		end
		if maxRosterPrefillCount > 10
			maxRosterPrefillCount = 10
		end
	end
	row=findfirst(isequal("backtraceWarningLimit"),configFile[:,1])
	if ! isnothing(row)
		if isinteger(configFile[row,2])
			backtraceWarningLimit = configFile[row,2]
		end
		if backtraceWarningLimit < 10
			backtraceWarningLimit = 10
		end
		if backtraceWarningLimit > 10000000
			backtraceWarningLimit = 10000000
		end
		for i = 1:7
			if (backtraceWarningLimit/10^i) < 1.0
				global backtraceWarningLimit = 10^(i-1)
				break
			end
		end
	end
	row=findfirst(isequal("classesInRandomOrder"),configFile[:,1])
	if ! isnothing(row)
		if isinteger(configFile[row,2])
			classesInRandomOrder = configFile[row,2]
		end
		if classesInRandomOrder <= 0
			classesInRandomOrder = 0
		end
	end
	row=findfirst(isequal("coursesInOrderOfTeacherCoverage"),configFile[:,1])
	if ! isnothing(row)
		if configFile[row,2] != 0
			coursesInOrderOfTeacherCoverage = true
		end
	end
end
println("relaxTeacherAvailability=",relaxTeacherAvailability)
println("maxRosterPrefillCount=",maxRosterPrefillCount)
println("backtraceWarningLimit=",backtraceWarningLimit)
println("classesInRandomOrder=",classesInRandomOrder)
println("coursesInOrderOfTeacherCoverage=",coursesInOrderOfTeacherCoverage)


println("reading: "*"tageStunden"*small*".csv")
if ! isfile("tageStunden"*small*".csv")
	println("Please edit days and hours")
end
tageStunden=prune(readdlm("tageStunden"*small*".csv",';'))
tage=Array{Int}(undef,0)
tageNamen=Array{String}(undef,0)
tageIndex = 1
for tag in tageStunden[2:end,1]
	global tageIndex
	if tag != 0
		push!(tage,tageIndex)
		push!(tageNamen,string(tag))
		tageIndex+=1
	end
end
stunden=Array{Int}(undef,0)
stundenNamen=Array{String}(undef,0)
stundenIndex = 1
for stunde in tageStunden[2:end,2]
	global stundenIndex
	if stunde != 0
		push!(stunden,stundenIndex)
		push!(stundenNamen,string(stunde))
		stundenIndex+=1
	end
end

println("###PROGRESS###",progress+=1)
if isfile("abort")
	println("processing aborted.")
	return
end

println("reading: "*"faecher"*small*".csv")
if ! isfile("faecher"*small*".csv")
	println("Please edit subjects")
end
faecher=prune(readdlm("faecher"*small*".csv",';'))
anzahlFaecher=size(faecher,1)-1
#faecherNamen=convert(Array{String},faecher[2:(anzahlFaecher+1),1])
faecherNamen=string.(faecher[2:(anzahlFaecher+1),1])
faecherTagMaxStunden=convert(Array{Int},faecher[2:(anzahlFaecher+1),2])
faecherRaumMaxKlassen=convert(Array{Int},faecher[2:(anzahlFaecher+1),3])
#faecherTagMinStunden=convert(Array{Int},faecher[2:(anzahlFaecher+1),4])
#faecherTagMinStundenMax=convert(Array{Int},faecher[2:(anzahlFaecher+1),5])

println("###PROGRESS###",progress+=1)

println("reading: "*"klassen"*small*".csv")
if ! isfile("klassen"*small*".csv")
	println("Please edit classes")
end
klassen=prune(readdlm("klassen"*small*".csv",';'))
firstKlassenIndex=2
lastKlassenIndex=size(klassen,1)
klassenNamen=string.(klassen[firstKlassenIndex:lastKlassenIndex,1])
klassenFachStunden=convert(Array{Int},klassen[firstKlassenIndex:lastKlassenIndex,2:(anzahlFaecher+1)])
anzahlKlassen=size(klassenFachStunden,1)

println("###PROGRESS###",progress+=1)

println("reading: "*"lehrer"*small*".csv")
if ! isfile("lehrer"*small*".csv")
	println("Please edit teachers")
end
lehrer=prune(readdlm("lehrer"*small*".csv",';'))
firstLehrerIndex=2
lastLehrerIndex=size(lehrer,1)
lehrerNamen=string.(lehrer[firstLehrerIndex:lastLehrerIndex,1])
lehrerNNindex=[]
if sum(occursin.("N.N.",lehrerNamen))>0
	lehrerNNindex=LinearIndices(lehrerNamen)[occursin.("N.N.",lehrerNamen)]
end
lehrerMaxStunden=convert(Array{Int},lehrer[firstLehrerIndex:lastLehrerIndex,2])
lehrerFachStunden=convert(Array{Int},lehrer[firstLehrerIndex:lastLehrerIndex,3:(anzahlFaecher+2)])
anzahlLehrer=length(lehrerNamen)
faecherToLehrer=[ (1:anzahlLehrer)[lehrerFachStunden[:,i].!=0] for i = 1:anzahlFaecher ]

println("###PROGRESS###",progress+=1)

println("reading: "*"klassenStundenFest"*small*".csv")
if ! isfile("klassenStundenFest"*small*".csv")
	klassenStundenFest=Array{String}(undef,1,5)
	klassenStundenFest.=""
else
	klassenStundenFest=prune(readdlm("klassenStundenFest"*small*".csv",';'))
end
klassenStundenFest=string.(klassenStundenFest)

klassenFachKonnektor=zeros(Int,anzahlKlassen,anzahlFaecher)
konnektorIDs=unique(sort(klassenStundenFest[2:end,2]))
for kIndex in eachindex(konnektorIDs)
	konnektorID=konnektorIDs[kIndex]
	klassenStundenFestLineIndices=findall(klassenStundenFest[:,2].==konnektorID)
	for klassenStundenFestLineIndex in klassenStundenFestLineIndices
		klasse=klassenStundenFest[klassenStundenFestLineIndex,1]
		#klassenIndex=findfirst(klassen[2:end,1].==klasse)
		klassenIndex=findfirst(klassenNamen.==klasse)
		fach=klassenStundenFest[klassenStundenFestLineIndex,3]
		#fachIndex=findfirst(faecher[2:end,1].==fach)
		fachIndex=findfirst(faecherNamen.==fach)
		if !isnothing(klassenIndex) && !isnothing(fachIndex)
			klassenFachKonnektor[klassenIndex,fachIndex]=kIndex
		end
	end
end

println("###PROGRESS###",progress+=1)

klassenFachKonnektorLookup=Dict{Int,Array{Array{Int,1},1}}()
klassenFachKonnektorCount=Dict{Array{Int,1},Int}()
for i in 1:anzahlKlassen
	for j in 1:anzahlFaecher
		if klassenFachKonnektor[i,j]>0
			if haskey(klassenFachKonnektorLookup,klassenFachKonnektor[i,j])
				append!(klassenFachKonnektorLookup[klassenFachKonnektor[i,j]],[[i,j]])
			else
				klassenFachKonnektorLookup[klassenFachKonnektor[i,j]]=[[i,j]]
			end
		end
	end
end

println("###PROGRESS###",progress+=1)

println("reading: "*"klasseFachLehrer"*small*".csv")
if ! isfile("klasseFachLehrer"*small*".csv")
	klasseFachLehrer=Array{String}(undef,anzahlKlassen+1,anzahlFaecher+1)
	klasseFachLehrer.=""
else
	klasseFachLehrer=prune(readdlm("klasseFachLehrer"*small*".csv",';'))
end
klasseFachLehrer=string.(klasseFachLehrer)

for fachIndex in eachindex(faecherNamen)
	global klasseFachLehrer
	fach=faecherNamen[fachIndex]
	oldIndex=findfirst(klasseFachLehrer[1,:].==fach)
	if isnothing(oldIndex)
		klasseFachLehrer=[ klasseFachLehrer[:,1:fachIndex] Array{Any}(undef,size(klasseFachLehrer)[1],1) klasseFachLehrer[:,fachIndex+1:end] ]
		klasseFachLehrer[:,fachIndex+1].="0"
		klasseFachLehrer[1,fachIndex+1]=fach
	else
		if oldIndex != fachIndex+1
			klasseFachLehrer=[ klasseFachLehrer[:,1:fachIndex] klasseFachLehrer[:,oldIndex] klasseFachLehrer[:,fachIndex+1:end] ]
			klasseFachLehrer=[ klasseFachLehrer[:,1:oldIndex] klasseFachLehrer[:,oldIndex+2:end] ] 
		end
	end
end
while anzahlFaecher+1 < size(klasseFachLehrer)[2]
	global klasseFachLehrer=klasseFachLehrer[:,1:end-1]
end
for klassenIndex in eachindex(klassenNamen)
	global klasseFachLehrer
	klasse=klassenNamen[klassenIndex]
	oldIndex=findfirst(klasseFachLehrer[:,1].==klasse)
	if isnothing(oldIndex)
		insert=Array{Any}(undef,1,size(klasseFachLehrer)[2])
		insert.="0"
		insert[1,1]=klasse
		klasseFachLehrer=[ klasseFachLehrer[1:klassenIndex,:] ; insert ; klasseFachLehrer[klassenIndex+1:end,:] ]
	else
		if oldIndex != klassenIndex+1
			klasseFachLehrer=[ klasseFachLehrer[1:klassenIndex,:] ; klasseFachLehrer[oldIndex:oldIndex,:] ; klasseFachLehrer[klassenIndex+1:end,:] ]
			klasseFachLehrer=[ klasseFachLehrer[1:oldIndex,:] ; klasseFachLehrer[oldIndex+2:end,:] ]
		end
	end
end
while anzahlKlassen+1 < size(klasseFachLehrer)[1]
	global klasseFachLehrer=klasseFachLehrer[1:end-1,:]
end

klasseCourseLehrer=zeros(Int,anzahlKlassen,anzahlFaecher)
for i in 2:(anzahlKlassen+1)
	for j in 2:(anzahlFaecher+1)
		tmpLehrer=klasseFachLehrer[i,j]
		lehrerIndex=LinearIndices(lehrerNamen)[lehrerNamen.==tmpLehrer]
		if length(lehrerIndex)==1
			klasseCourseLehrer[i-1,j-1]=lehrerIndex[1]
		else
			klasseCourseLehrer[i-1,j-1]=0
		end
	end
end

println("###PROGRESS###",progress+=1)

klassenStundenFestLineIndices=findall(klassenStundenFest[:,4].!="0")
stundenFest=Array{Any}(undef,length(klassenStundenFestLineIndices),length(klassenStundenFest[1,:]))
stundenFestRowIndex=2
for klassenStundenFestLineIndex in klassenStundenFestLineIndices[2:end]
	global stundenFestRowIndex
	stundenFest[stundenFestRowIndex,1]=klassenStundenFest[klassenStundenFestLineIndex,1]
	klasse=klassenStundenFest[klassenStundenFestLineIndex,1]
	#klassenIndex=findfirst(klassen[2:end,1].==klasse)
	klassenIndex=findfirst(klassenNamen.==klasse)
	tag=klassenStundenFest[klassenStundenFestLineIndex,4]
	tagIndex=findfirst(tageNamen.==tag)
	stundenFest[stundenFestRowIndex,2]=tagIndex
	stunde=klassenStundenFest[klassenStundenFestLineIndex,5]
	stundeIndex=findfirst(stundenNamen.==stunde)
	stundenFest[stundenFestRowIndex,3]=stundeIndex
	stundenFest[stundenFestRowIndex,4]=klassenStundenFest[klassenStundenFestLineIndex,3]
	fach=klassenStundenFest[klassenStundenFestLineIndex,3]
	#fachIndex=findfirst(faecher[2:end,1].==fach)
	fachIndex=findfirst(faecherNamen.==fach)
	if klasseCourseLehrer[klassenIndex,fachIndex]==0 
		stundenFest[stundenFestRowIndex,5]=klasseCourseLehrer[klassenIndex,fachIndex]
	else
		stundenFest[stundenFestRowIndex,5]=lehrerNamen[klasseCourseLehrer[klassenIndex,fachIndex]]
	end
	stundenFestRowIndex+=1
end

println("###PROGRESS###",progress+=1)

klasseCoursePreset=zeros(Int,size(stundenFest)[1]-1,size(stundenFest)[2])
for i in 2:size(stundenFest)[1]
	klassenIndex=LinearIndices(klassenNamen)[klassenNamen.==stundenFest[i,1]]
	length(klassenIndex)==1 ? klasseCoursePreset[i-1,1]=klassenIndex[1] : klasseCoursePreset[i-1,1]=0
	klasseCoursePreset[i-1,2]=stundenFest[i,2]
	klasseCoursePreset[i-1,3]=stundenFest[i,3]
	courseIndex=LinearIndices(faecherNamen)[faecherNamen.==stundenFest[i,4]]
	length(courseIndex)==1 ? klasseCoursePreset[i-1,4]=courseIndex[1] : klasseCoursePreset[i-1,4]=0
	lehrerIndex=LinearIndices(lehrerNamen)[lehrerNamen.==stundenFest[i,5]]
	length(lehrerIndex)==1 ? klasseCoursePreset[i-1,5]=lehrerIndex[1] : klasseCoursePreset[i-1,5]=0
end

println("###PROGRESS###",progress+=1)

klassenFachStundenBelegt=zeros(Int,anzahlKlassen,anzahlFaecher);                      
klassenFachLehrerBelegt=zeros(Int,anzahlKlassen,anzahlFaecher);                       
klassenFachLehrerAnzahl=zeros(Int,anzahlKlassen,anzahlFaecher,anzahlLehrer);          
lehrerTagStundeBelegt=falses(anzahlLehrer,maximum(tage),maximum(stunden));            
lehrerStundenBelegt=zeros(Int,anzahlLehrer);                                          
lehrerFachStundenBelegt=zeros(Int,anzahlLehrer,anzahlFaecher);                        
lehrerFachStundenBelegtSumme=zeros(Int,anzahlLehrer,anzahlFaecher);                   
klassenTagFachStundenBelegt=zeros(Int,anzahlKlassen,length(tage),anzahlFaecher);      
faecherTagMaxStundenBelegt=zeros(Int,maximum(tage),maximum(stunden),anzahlFaecher);   
rosterKonnekt=zeros(Int,maximum(stunden),maximum(tage),anzahlKlassen);                
rosterCount=zeros(Int,maximum(stunden),maximum(tage),anzahlKlassen);                  
klassenFachKonnektorCount;                                                            
backtraceMarker=Array{Tuple{Int,Int,Int},1}();                                        
push!(backtraceMarker,(-1,-1,-1))
allBacktraceMarkerCourse=Dict{Int,Array{Tuple{Int,Int,Int},1}}();                     
#rosterDoubleCourses=zeros(Int,maximum(stunden),maximum(tage),anzahlKlassen);         
#klassenDoubleCourseBelegt=zeros(Int,anzahlKlassen,anzahlFaecher);                    


rosterPrefill=fill([0,0],maximum(stunden),maximum(tage),anzahlKlassen);
allRostersPrefill=Dict{Int64,typeof(rosterPrefill)}()

println("###PROGRESS###",progress+=1)

function setCoursePresets()
	for i in 1:size(klasseCoursePreset)[1]
		klassenIndex=klasseCoursePreset[i,1]
		tagIndex=klasseCoursePreset[i,2]
		stundenIndex=klasseCoursePreset[i,3]
		courseIndex=klasseCoursePreset[i,4]
		teacherIndex=klasseCoursePreset[i,5]
		if klassenIndex>0 && stundenIndex>0 && tagIndex>0
			rosterPrefill[stundenIndex,tagIndex,klassenIndex]=[courseIndex,teacherIndex]
			setSpecificHelper(klassenIndex,teacherIndex,courseIndex,tagIndex,stundenIndex)
		end
	end
end

println("###PROGRESS###",progress+=1)

function resetAllHelpers()
	fill!(rosterPrefill,[0,0])
	#fill!(roster,[0,0])
	fill!(klassenFachStundenBelegt,0)
	fill!(klassenFachLehrerBelegt,0)
	fill!(klassenFachLehrerAnzahl,0)
	fill!(lehrerTagStundeBelegt,false)
	fill!(lehrerStundenBelegt,0)
	fill!(lehrerFachStundenBelegt,0)
	fill!(lehrerFachStundenBelegtSumme,0)
	fill!(klassenTagFachStundenBelegt,0)
	fill!(faecherTagMaxStundenBelegt,0)
	fill!(rosterKonnekt,0)
	fill!(rosterCount,0)
	empty!(klassenFachKonnektorCount)
	empty!(backtraceMarker)
	push!(backtraceMarker,(-1,-1,-1))
	empty!(allBacktraceMarkerCourse)
	#fill!(rosterDoubleCourses,0)
	#fill!(klassenDoubleCourseBelegt,0)
	setCoursePresets()
	nothing
end

println("###PROGRESS###",progress+=1)

function resetHelpersNewCourse()
	empty!(backtraceMarker)
	push!(backtraceMarker,(-1,-1,-1))
	nothing
end

println("###PROGRESS###",progress+=1)

function checkTeacherSum(teacherIndex::Int,courseIndex::Int,klassenIndex::Int)
	r=  true &&
		teacherIndex > 0 && teacherIndex <= anzahlLehrer &&
		courseIndex > 0 && courseIndex <= anzahlFaecher &&
		klassenIndex > 0 && klassenIndex <= anzahlKlassen &&
		klassenFachStunden[klassenIndex,courseIndex] <= lehrerFachStunden[teacherIndex,courseIndex]-lehrerFachStundenBelegtSumme[teacherIndex,courseIndex] &&
		klassenFachStunden[klassenIndex,courseIndex] <= lehrerMaxStunden[teacherIndex]-lehrerStundenBelegt[teacherIndex]
	r
end

println("###PROGRESS###",progress+=1)

function checkTeacherAvailable(teacherIndex::Int,courseIndex::Int,tagIndex::Int,stundenIndex::Int)
	r=  true &&
		teacherIndex > 0 && teacherIndex <= anzahlLehrer &&
		courseIndex > 0 && courseIndex <= anzahlFaecher &&
		tagIndex > 0 && tagIndex <= length(tage) &&
		stundenIndex > 0 && stundenIndex <= length(stunden) &&
		(   issubset([teacherIndex],lehrerNNindex) ||
			(
				!lehrerTagStundeBelegt[teacherIndex,tage[tagIndex],stunden[stundenIndex]] &&
				lehrerStundenBelegt[teacherIndex] < lehrerMaxStunden[teacherIndex] &&
				lehrerFachStundenBelegt[teacherIndex,courseIndex] < lehrerFachStunden[teacherIndex,courseIndex] 
			)
		)
	r
end

println("###PROGRESS###",progress+=1)

function checkCourseAvailableKonnekt(klassenIndex::Int,courseIndex::Int,tagIndex::Int,stundenIndex::Int)
	r=  true &&
		klassenIndex > 0 && klassenIndex <= anzahlKlassen &&
		courseIndex > 0 && courseIndex <= anzahlFaecher &&
		tagIndex > 0 && tagIndex <= length(tage) &&
		stundenIndex > 0 && stundenIndex <= length(stunden) &&
		klassenFachStundenBelegt[klassenIndex,courseIndex]<klassenFachStunden[klassenIndex,courseIndex] &&
		klassenTagFachStundenBelegt[klassenIndex,tage[tagIndex],courseIndex]<faecherTagMaxStunden[courseIndex]
	r
end

println("###PROGRESS###",progress+=1)

function checkCourseAvailableKonnektBack(klassenIndex::Int,courseIndex::Int,tagIndex::Int,stundenIndex::Int)
	r=true
	if  klassenIndex > 0 && klassenIndex <= anzahlKlassen &&
		courseIndex > 0 && courseIndex <= anzahlFaecher && 
		klassenFachKonnektor[klassenIndex,courseIndex]>0
			konnekt=klassenFachKonnektor[klassenIndex,courseIndex]
			konnektList=klassenFachKonnektorLookup[konnekt]
			for klasseFach in konnektList
				if  tagIndex > 0 && tagIndex <= length(tage) &&
					stundenIndex > 0 && stundenIndex <= length(stunden) &&
					klasseFach[1]<klassenIndex && 
					rosterPrefill[stunden[stundenIndex],tage[tagIndex],klasseFach[1]][1] != klasseFach[2] &&
					rosterPrefill[stunden[stundenIndex],tage[tagIndex],klasseFach[1]][1] != 0
						r=false
				end
			end
	end
	r
end

function checkCourseAvailableKonnektAhead(klassenIndex::Int,courseIndex::Int,tagIndex::Int,stundenIndex::Int)
	r=true
	if  klassenIndex > 0 && klassenIndex <= anzahlKlassen &&
		courseIndex > 0 && courseIndex <= anzahlFaecher && 
		klassenFachKonnektor[klassenIndex,courseIndex]>0
			konnekt=klassenFachKonnektor[klassenIndex,courseIndex]
			konnektList=klassenFachKonnektorLookup[konnekt]
			for klasseFach in konnektList
				if  tagIndex > 0 && tagIndex <= length(tage) &&
					stundenIndex > 0 && stundenIndex <= length(stunden) &&
					klasseFach[1]>klassenIndex &&
					rosterPrefill[stunden[stundenIndex],tage[tagIndex],klasseFach[1]][1] != klasseFach[2] &&
					rosterPrefill[stunden[stundenIndex],tage[tagIndex],klasseFach[1]][1] != 0
						r=false
				end
			end
	end
	r
end

println("###PROGRESS###",progress+=1)

function checkCourseAvailable(klassenIndex::Int,courseIndex::Int,tagIndex::Int,stundenIndex::Int)
	r=  true &&
		klassenIndex > 0 && klassenIndex <= anzahlKlassen &&
		courseIndex > 0 && courseIndex <= anzahlFaecher &&
		tagIndex > 0 && tagIndex <= length(tage) &&
		stundenIndex > 0 && stundenIndex <= length(stunden) &&
		klassenFachStundenBelegt[klassenIndex,courseIndex]<klassenFachStunden[klassenIndex,courseIndex] &&
		klassenTagFachStundenBelegt[klassenIndex,tage[tagIndex],courseIndex]<faecherTagMaxStunden[courseIndex] &&
		(   false ||
			faecherRaumMaxKlassen[courseIndex]==0 ||
			faecherTagMaxStundenBelegt[tage[tagIndex],stunden[stundenIndex],courseIndex]<faecherRaumMaxKlassen[courseIndex]
		) &&
		checkCourseAvailableKonnektBack(klassenIndex,courseIndex,tagIndex,stundenIndex) &&
		checkCourseAvailableKonnektAhead(klassenIndex,courseIndex,tagIndex,stundenIndex)
	r
end

println("###PROGRESS###",progress+=1)

function resetSpecificHelper(klassenIndex::Int,teacherIndex::Int,courseIndex::Int,tagIndex::Int,stundenIndex::Int)
	if  courseIndex > 0 && courseIndex <= anzahlFaecher &&
		klassenIndex > 0 && klassenIndex <= anzahlKlassen &&
		tagIndex > 0 && tagIndex <= length(tage) &&
		stundenIndex > 0 && stundenIndex <= length(stunden)
			klassenFachStundenBelegt[klassenIndex,courseIndex]=klassenFachStundenBelegt[klassenIndex,courseIndex]-1
			klassenTagFachStundenBelegt[klassenIndex,tage[tagIndex],courseIndex]=klassenTagFachStundenBelegt[klassenIndex,tage[tagIndex],courseIndex]-1
			faecherTagMaxStundenBelegt[tage[tagIndex],stunden[stundenIndex],courseIndex]=faecherTagMaxStundenBelegt[tage[tagIndex],stunden[stundenIndex],courseIndex]-1
			if klassenFachKonnektor[klassenIndex,courseIndex]>0
				konnekt=klassenFachKonnektor[klassenIndex,courseIndex]
				konnektKey=[konnekt,tage[tagIndex],stunden[stundenIndex]]
				if haskey(klassenFachKonnektorCount,konnektKey)
					klassenFachKonnektorCount[konnektKey]=klassenFachKonnektorCount[konnektKey]-1
					if klassenFachKonnektorCount[konnektKey]==0
						konnektList=klassenFachKonnektorLookup[konnekt]
						for klasseFach in konnektList
							rosterKonnekt[stunden[stundenIndex],tage[tagIndex],klasseFach[1]]=0
						end
					end
				end
			end
	end
	if  teacherIndex > 0 && teacherIndex <= anzahlLehrer &&
		tagIndex > 0 && tagIndex <= length(tage) &&
		stundenIndex > 0 && stundenIndex <= length(stunden)
			lehrerTagStundeBelegt[teacherIndex,tage[tagIndex],stunden[stundenIndex]]=false
			lehrerStundenBelegt[teacherIndex]=lehrerStundenBelegt[teacherIndex]-1
	end
	if  teacherIndex > 0 && teacherIndex <= anzahlLehrer &&
		courseIndex > 0 && courseIndex <= anzahlFaecher &&
		klassenIndex > 0 && klassenIndex <= anzahlKlassen
			if klassenFachLehrerAnzahl[klassenIndex,courseIndex,teacherIndex]>0
				klassenFachLehrerAnzahl[klassenIndex,courseIndex,teacherIndex]=klassenFachLehrerAnzahl[klassenIndex,courseIndex,teacherIndex]-1
			end
			if klassenFachLehrerAnzahl[klassenIndex,courseIndex,teacherIndex]==0
				klassenFachLehrerBelegt[klassenIndex,courseIndex]=0
				lehrerFachStundenBelegtSumme[teacherIndex,courseIndex]=lehrerFachStundenBelegtSumme[teacherIndex,courseIndex]-klassenFachStunden[klassenIndex,courseIndex]
			end
			if lehrerFachStundenBelegt[teacherIndex,courseIndex]>0
				lehrerFachStundenBelegt[teacherIndex,courseIndex]=lehrerFachStundenBelegt[teacherIndex,courseIndex]-1
			end
	end
	nothing
end

println("###PROGRESS###",progress+=1)

function setSpecificHelper(klassenIndex::Int,teacherIndex::Int,courseIndex::Int,tagIndex::Int,stundenIndex::Int)
	if  courseIndex > 0 && courseIndex <= anzahlFaecher &&
		klassenIndex > 0 && klassenIndex <= anzahlKlassen &&
		tagIndex > 0 && tagIndex <= length(tage) &&
		stundenIndex > 0 && stundenIndex <= length(stunden)
			klassenFachStundenBelegt[klassenIndex,courseIndex]=klassenFachStundenBelegt[klassenIndex,courseIndex]+1
			klassenTagFachStundenBelegt[klassenIndex,tage[tagIndex],courseIndex]=klassenTagFachStundenBelegt[klassenIndex,tage[tagIndex],courseIndex]+1
			faecherTagMaxStundenBelegt[tage[tagIndex],stunden[stundenIndex],courseIndex]=faecherTagMaxStundenBelegt[tage[tagIndex],stunden[stundenIndex],courseIndex]+1
			if klassenFachKonnektor[klassenIndex,courseIndex]>0
				konnekt=klassenFachKonnektor[klassenIndex,courseIndex]
				konnektList=klassenFachKonnektorLookup[konnekt]
				for klasseFach in konnektList
					if klassenFachStunden[klasseFach[1],klasseFach[2]]>0
						rosterKonnekt[stunden[stundenIndex],tage[tagIndex],klasseFach[1]]=klasseFach[2]
					end
				end
				konnektKey=[konnekt,tage[tagIndex],stunden[stundenIndex]]
				if haskey(klassenFachKonnektorCount,konnektKey)
					klassenFachKonnektorCount[konnektKey]=klassenFachKonnektorCount[konnektKey]+1
				else
					klassenFachKonnektorCount[konnektKey]=1
				end
			end
	end
	if  teacherIndex > 0 && teacherIndex <= anzahlLehrer &&
		tagIndex > 0 && tagIndex <= length(tage) &&
		stundenIndex > 0 && stundenIndex <= length(stunden)
			lehrerTagStundeBelegt[teacherIndex,tage[tagIndex],stunden[stundenIndex]]=true
			lehrerStundenBelegt[teacherIndex]=lehrerStundenBelegt[teacherIndex]+1
	end
	if  teacherIndex > 0 && teacherIndex <= anzahlLehrer &&
		courseIndex > 0 && courseIndex <= anzahlFaecher &&
		klassenIndex > 0 && klassenIndex <= anzahlKlassen
			klassenFachLehrerAnzahl[klassenIndex,courseIndex,teacherIndex]=klassenFachLehrerAnzahl[klassenIndex,courseIndex,teacherIndex]+1
			if klassenFachLehrerBelegt[klassenIndex,courseIndex]==0
				lehrerFachStundenBelegtSumme[teacherIndex,courseIndex]=lehrerFachStundenBelegtSumme[teacherIndex,courseIndex]+klassenFachStunden[klassenIndex,courseIndex]
			end
			klassenFachLehrerBelegt[klassenIndex,courseIndex]=teacherIndex
			lehrerFachStundenBelegt[teacherIndex,courseIndex]=lehrerFachStundenBelegt[teacherIndex,courseIndex]+1
	end
	nothing
end

println("###PROGRESS###",progress+=1)

function backtraceOneStep(permutedKlassenIndex::Int,tagIndex::Int,stundenIndex::Int)
	permutedKlassenIndex-=1
	if permutedKlassenIndex<=0
		permutedKlassenIndex=anzahlKlassen
		tagIndex-=1
		if tagIndex<=0
			tagIndex=length(tage)
			stundenIndex-=1
			if stundenIndex<=0
				permutedKlassenIndex=-1
				tagIndex=-1
				stundenIndex=-1
			end
		end
	end
	(permutedKlassenIndex,tagIndex,stundenIndex)
end

println("###PROGRESS###",progress+=1)

function cmpTriple(t1,t2)
	(permutedKlassenIndex1::Int,tagIndex1::Int,stundenIndex1::Int)=t1
	(permutedKlassenIndex2::Int,tagIndex2::Int,stundenIndex2::Int)=t2
	if stundenIndex1<stundenIndex2
		r=-1
	end
	if stundenIndex1>stundenIndex2
		r=1
	end
	if stundenIndex1==stundenIndex2
		if tagIndex1<tagIndex2
			r=-1
		end
		if tagIndex1>tagIndex2
			r=1
		end
		if tagIndex1==tagIndex2
			if permutedKlassenIndex1<permutedKlassenIndex2
				r=-1
			end
			if permutedKlassenIndex1>permutedKlassenIndex2
				r=1
			end
			if permutedKlassenIndex1==permutedKlassenIndex2
				r=0
			end
		end
	end
	r
end

println("###PROGRESS###",progress+=1)

function isPreset(klassenIndex::Int,tagIndex::Int,stundenIndex::Int)
	r=  true &&
		klassenIndex > 0 && klassenIndex <= anzahlKlassen &&
		tagIndex > 0 && tagIndex <= length(tage) &&
		stundenIndex > 0 && stundenIndex <= length(stunden) &&
		sum([ (klasseCoursePreset[klasseCoursePresetIndex,1],klasseCoursePreset[klasseCoursePresetIndex,2],klasseCoursePreset[klasseCoursePresetIndex,3])==(klassenIndex,tagIndex,stundenIndex) for klasseCoursePresetIndex in 1:(size(klasseCoursePreset)[1]) ]) > 0
	r
end

println("###PROGRESS###",progress+=1)

function getNextTeacherCourse(course::Int,lehrerToUsePerClass::Array{Array{Int,1}},klassenIndex::Int,permutedKlassenIndex::Int,tagIndex::Int,stundenIndex::Int,doBacktrace::Bool)
	global relaxTeacherAvailability
	currentCourseIndex=rosterPrefill[stunden[stundenIndex],tage[tagIndex],klassenIndex][1]
	currentTeacherIndex=rosterPrefill[stunden[stundenIndex],tage[tagIndex],klassenIndex][2]
	if doBacktrace
		# backtracing
		nextCourseIndex=-1
		nextTeacherIndex=-1
		if  !isPreset(klassenIndex,tagIndex,stundenIndex) && 
			currentCourseIndex==course
				resetSpecificHelper(klassenIndex,currentTeacherIndex,currentCourseIndex,tagIndex,stundenIndex)
				if klassenFachLehrerBelegt[klassenIndex,currentCourseIndex]==0 && klasseCourseLehrer[klassenIndex,course]==0
					i=1
					foundCurrent=false
					while i<=length(lehrerToUsePerClass[permutedKlassenIndex]) && nextCourseIndex<0
						if foundCurrent
							nextTeacherIndex=lehrerToUsePerClass[permutedKlassenIndex][i]
							if checkTeacherAvailable(nextTeacherIndex,currentCourseIndex,tagIndex,stundenIndex) && checkTeacherSum(nextTeacherIndex,currentCourseIndex,klassenIndex)
								nextCourseIndex=currentCourseIndex
								push!(backtraceMarker,(permutedKlassenIndex,tagIndex,stundenIndex))
							else
								nextCourseIndex=-1
							end
						end
						if lehrerToUsePerClass[permutedKlassenIndex][i]==currentTeacherIndex
							foundCurrent=true
						end
						i+=1
					end
				end
				if nextCourseIndex<0
					nextCourseIndex=0
					nextTeacherIndex=0
				end
		end
	else
		nextCourseIndex=-1
		nextTeacherIndex=0
		if  !isPreset(klassenIndex,tagIndex,stundenIndex) &&
			( currentCourseIndex==course || ( currentCourseIndex==0 && currentTeacherIndex==0 ) )
				thisCourseUnsetClasses=klassenFachStunden[klassenIndex,course]-klassenFachStundenBelegt[klassenIndex,course]
				if thisCourseUnsetClasses>0
					if rosterKonnekt[stunden[stundenIndex],tage[tagIndex],klassenIndex]>0
						nextCourseIndex=rosterKonnekt[stunden[stundenIndex],tage[tagIndex],klassenIndex]
						if nextCourseIndex==course
							if  ! checkCourseAvailableKonnekt(klassenIndex,nextCourseIndex,tagIndex,stundenIndex)
								nextCourseIndex=-1
								nextTeacherIndex=-1
							else
								nextTeacherIndex=klassenFachLehrerBelegt[klassenIndex,nextCourseIndex]
								if nextTeacherIndex>0
									if ! relaxTeacherAvailability && ! checkTeacherAvailable(nextTeacherIndex,nextCourseIndex,tagIndex,stundenIndex)
										nextCourseIndex=-1
										nextTeacherIndex=-1
									end
								else
									nextTeacherIndex=klasseCourseLehrer[klassenIndex,course]
									if nextTeacherIndex>0
										if ! relaxTeacherAvailability && ! checkTeacherAvailable(nextTeacherIndex,nextCourseIndex,tagIndex,stundenIndex)
											nextCourseIndex=-1
											nextTeacherIndex=-1
										end
									else
										nextTeacherIndex=-1
										j=1
										while j<=length(faecherToLehrer[nextCourseIndex]) && nextTeacherIndex<0
											nextTeacherIndex=faecherToLehrer[nextCourseIndex][j]
											if ! checkTeacherSum(nextTeacherIndex,nextCourseIndex,klassenIndex) || ! checkTeacherAvailable(nextTeacherIndex,nextCourseIndex,tagIndex,stundenIndex)
												nextTeacherIndex=-1
											end
											j+=1
										end
										if nextTeacherIndex<0
											nextCourseIndex=-1
										end
									end
								end
							end
						else
							nextCourseIndex=-1
						end
					else
						nextCourseIndex=course
						if checkCourseAvailable(klassenIndex,nextCourseIndex,tagIndex,stundenIndex)
							nextTeacherIndex=klassenFachLehrerBelegt[klassenIndex,nextCourseIndex]
							if nextTeacherIndex>0
								if ! relaxTeacherAvailability && ! checkTeacherAvailable(nextTeacherIndex,nextCourseIndex,tagIndex,stundenIndex)
									nextCourseIndex=0
									nextTeacherIndex=0
								else
									push!(backtraceMarker,(permutedKlassenIndex,tagIndex,stundenIndex))
								end
							else
								nextTeacherIndex=klasseCourseLehrer[klassenIndex,course]
								if nextTeacherIndex>0
									if ! relaxTeacherAvailability && ! checkTeacherAvailable(nextTeacherIndex,nextCourseIndex,tagIndex,stundenIndex)
										nextCourseIndex=0
										nextTeacherIndex=0
									else
										push!(backtraceMarker,(permutedKlassenIndex,tagIndex,stundenIndex))
									end
								else
									nextTeacherIndex=-1
									j=1
									while j<=length(lehrerToUsePerClass[permutedKlassenIndex]) && nextTeacherIndex<0
										nextTeacherIndex=lehrerToUsePerClass[permutedKlassenIndex][j]
										if ! checkTeacherSum(nextTeacherIndex,nextCourseIndex,klassenIndex) || ! checkTeacherAvailable(nextTeacherIndex,nextCourseIndex,tagIndex,stundenIndex)
											nextTeacherIndex=-1
										end
										j+=1
									end
									if nextTeacherIndex<0
										nextTeacherIndex=0
										nextCourseIndex=0
									else
										push!(backtraceMarker,(permutedKlassenIndex,tagIndex,stundenIndex))
									end
								end
							end
						else
							nextCourseIndex=0
							nextTeacherIndex=0
						end
					end
				end
		end
	end
	(nextCourseIndex,nextTeacherIndex)
end

println("###PROGRESS###",progress+=1)

function fillRoster(permutedKlassenIndices::Array{Int,1},course::Int,lehrerToUsePerClass::Array{Array{Int,1}},tage::Array{Int,1},stunden::Array{Int,1},doBacktrace::Bool)
	global deadEnd
	backtraceLog=Dict{String,Int}()
	btDevider=Dict{String,Int}()
	stundenIndex=1
	tagIndex=1
	permutedKlassenIndex=1
	(nextCourseIndex,nextTeacherIndex)=(0,0)
	if doBacktrace
		stundenIndex=length(stunden)
		tagIndex=length(tage)
		permutedKlassenIndex=anzahlKlassen
	end
	while stundenIndex<=length(stunden) && stundenIndex>0 && ! deadEnd
		doBacktrace ? tagIndex : tagIndex=1
		while tagIndex<=length(tage) && tagIndex>0 && ! deadEnd
			doBacktrace ? permutedKlassenIndex : permutedKlassenIndex=1
			while permutedKlassenIndex <= anzahlKlassen && permutedKlassenIndex > 0 && ! deadEnd
				klassenIndex=permutedKlassenIndices[permutedKlassenIndex]
				if isfile("abort")
					println("processing aborted.")
					deadEnd = true
					break
				end
				(nextCourseIndex,nextTeacherIndex)=getNextTeacherCourse(course,lehrerToUsePerClass,klassenIndex,permutedKlassenIndex,tagIndex,stundenIndex,doBacktrace)
				global globalCount+=1
				if (nextCourseIndex,nextTeacherIndex)==(-1,-1)
					btString=faecherNamen[course]*"-"*klassenNamen[klassenIndex]
					if ! haskey(backtraceLog,btString)
						backtraceLog[btString]=0
						btDevider[btString]=10
					end
					backtraceLog[btString]+=1
					if mod(backtraceLog[btString],btDevider[btString])==0
						btDevider[btString]*=10
						println("Backtrace ",backtraceLog[btString]," times: ",btString)
						if mod(backtraceLog[btString],backtraceWarningLimit)==0
								println("###USERMSG###String166 "*btString)
						end
					end
					doBacktrace=true
					global globalBacktraceCount+=1
				else
					doBacktrace=false
					if (nextCourseIndex,nextTeacherIndex)!=(-1,0)
						rosterPrefill[stunden[stundenIndex],tage[tagIndex],klassenIndex]=[nextCourseIndex,nextTeacherIndex]
						setSpecificHelper(klassenIndex,nextTeacherIndex,nextCourseIndex,tagIndex,stundenIndex)
					end
					if stundenIndex==length(stunden) && tagIndex==length(tage) && klassenFachStunden[klassenIndex,course]-klassenFachStundenBelegt[klassenIndex,course]>0
						btString=faecherNamen[course]*"-"*klassenNamen[klassenIndex]
						if ! haskey(backtraceLog,btString)
							backtraceLog[btString]=0
							btDevider[btString]=10
						end
						backtraceLog[btString]+=1
						if mod(backtraceLog[btString],btDevider[btString])==0
							btDevider[btString]*=10
							println("Backtrace ",backtraceLog[btString]," times: ",btString)
							if mod(backtraceLog[btString],backtraceWarningLimit)==0
								println("###USERMSG###String166 "*btString)
							end
						end
						rosterPrefill[stunden[stundenIndex],tage[tagIndex],klassenIndex]=[0,0]
						resetSpecificHelper(klassenIndex,nextTeacherIndex,nextCourseIndex,tagIndex,stundenIndex)
						doBacktrace=true
						global globalBacktraceCount+=1
					end
				end
				if doBacktrace
					(permutedKlassenIndex,tagIndex,stundenIndex)=backtraceOneStep(permutedKlassenIndex,tagIndex,stundenIndex)
					(bt_permutedKlassenIndex,bt_tagIndex,bt_stundenIndex)=pop!(backtraceMarker)
					while cmpTriple((permutedKlassenIndex,tagIndex,stundenIndex),(bt_permutedKlassenIndex,bt_tagIndex,bt_stundenIndex))>0
						klassenIndex=permutedKlassenIndices[permutedKlassenIndex]
						currentCourseIndex=rosterPrefill[stunden[stundenIndex],tage[tagIndex],klassenIndex][1]
						currentTeacherIndex=rosterPrefill[stunden[stundenIndex],tage[tagIndex],klassenIndex][2]
						if  currentCourseIndex==course && !isPreset(klassenIndex,tagIndex,stundenIndex)
							rosterPrefill[stunden[stundenIndex],tage[tagIndex],klassenIndex]=[0,0]
							resetSpecificHelper(klassenIndex,currentTeacherIndex,currentCourseIndex,tagIndex,stundenIndex)
						end
						(permutedKlassenIndex,tagIndex,stundenIndex)=backtraceOneStep(permutedKlassenIndex,tagIndex,stundenIndex)
					end
				end
				doBacktrace ? permutedKlassenIndex : permutedKlassenIndex+=1
			end
			doBacktrace ? tagIndex : tagIndex+=1
		end
		doBacktrace ? stundenIndex : stundenIndex+=1
	end
	(permutedKlassenIndex,tagIndex,stundenIndex)
end

function PermuteDay(stundeToFlatIndex::Int,tagFreeIndex::Int,klassenIndex::Int)
	newFreeStunde=0
	foundPermutation=false
	subjectBlocks=Array{Array{Int,1},1}()
	subjectBlock=Array{Int,1}()
	freeStunden=Array{Int,1}()
	lastCourse=0
	for stundenIndex=1:stundeToFlatIndex
		course=rosterPrefill[stundenIndex,tagFreeIndex,klassenIndex][1]
		if course!=0
			if lastCourse==0 || lastCourse==course
				push!(subjectBlock,stundenIndex)
			else
				push!(subjectBlocks,subjectBlock)
				subjectBlock=Array{Int,1}()
				push!(subjectBlock,stundenIndex)
			end
			lastCourse=course
		else
			push!(freeStunden,stundenIndex)
		end
	end
	if length(subjectBlock)>0
		push!(subjectBlocks,subjectBlock)
	end
	movePossible=false
	freeStunde=0
	for outer freeStunde in freeStunden
		for outer subjectBlock in reverse(subjectBlocks)
			if last(subjectBlock)<freeStunde
				moveDiff=freeStunde-last(subjectBlock)
				movePossible=true
				lastCheckedCourse=0
				lastCheckedTeacher=0
				for oldStunde in reverse(subjectBlock)
					newStunde=oldStunde+moveDiff
					course=rosterPrefill[oldStunde,tagFreeIndex,klassenIndex][1]
					teacher=rosterPrefill[oldStunde,tagFreeIndex,klassenIndex][2]
					resetSpecificHelper(klassenIndex,teacher,course,tagFreeIndex,oldStunde)
					if		(lastCheckedCourse==0 || lastCheckedCourse!=course) &&
							!checkCourseAvailable(klassenIndex,course,tagFreeIndex,newStunde)
						movePossible=false
					end
					if		(lastCheckedTeacher==0 || lastCheckedTeacher!=teacher) &&
							!checkTeacherAvailable(teacher,course,tagFreeIndex,newStunde)
						movePossible=false
					end
					if		isPreset(klassenIndex,tagFreeIndex,oldStunde)
						movePossible=false
					end
					setSpecificHelper(klassenIndex,teacher,course,tagFreeIndex,oldStunde)
					lastCheckedCourse=course
					lastCheckedTeacher=teacher
				end
				break
			end
		end
		if movePossible
			moveDiff=freeStunde-last(subjectBlock)
			for oldStunde in reverse(subjectBlock)
				newFreeStunde=oldStunde
				course=rosterPrefill[oldStunde,tagFreeIndex,klassenIndex][1]
				teacher=rosterPrefill[oldStunde,tagFreeIndex,klassenIndex][2]
				rosterPrefill[oldStunde,tagFreeIndex,klassenIndex]=[0,0]
				resetSpecificHelper(klassenIndex,teacher,course,tagFreeIndex,oldStunde)
				rosterPrefill[oldStunde+moveDiff,tagFreeIndex,klassenIndex]=[course,teacher]
				setSpecificHelper(klassenIndex,teacher,course,tagFreeIndex,oldStunde+moveDiff)
			end
			foundPermutation=true
			break
		end
	end
	#subjectBlocks,freeStunden,movePossible,freeStunde,subjectBlock
	(foundPermutation,newFreeStunde)
end

println("###PROGRESS###",progress+=1)

faecherStundenLehrer=fill(zeros(Float64,4),anzahlFaecher);
for course in 1:anzahlFaecher
	uStunden=sum(klassenFachStunden[:,course])
	lehrerStunden=sum(lehrerFachStunden[:,course])
	ratio=float(lehrerStunden)/float(uStunden)
	faecherStundenLehrer[course]=[course,uStunden,lehrerStunden,ratio]
end
if coursesInOrderOfTeacherCoverage
	sort!(faecherStundenLehrer,by=x->x[4])
end

println("###PROGRESS###",progress+=1)

resetAllHelpers()

if sum(klassenFachStunden)>sum(lehrerMaxStunden)
	println("FAIL: "*string(sum(lehrerMaxStunden))*" teacher hours < "*string(sum(klassenFachStunden))*" class hours")
	println("###USERMSG###String137")
else
	println("OK: "*string(sum(lehrerMaxStunden))*" teacher hours >= "*string(sum(klassenFachStunden))*" class hours")
end

lehrerFactor=lehrerMaxStunden./[ sum(lehrerFachStunden[lehrerIndex,:]) for lehrerIndex in 1:anzahlLehrer ]
lehrerFactor[isinf.(lehrerFactor)].=0
lehrerStundenProFach=[ sum(lehrerFachStunden[:,faecherIndex].*lehrerFactor) for faecherIndex in 1:anzahlFaecher ]
faecherStunden=[ sum(klassenFachStunden[:,faecherIndex]) for faecherIndex in 1:anzahlFaecher ]
lehrerFachRatio=lehrerStundenProFach./faecherStunden
for ratioIndex in sortperm(lehrerFachRatio)
	if lehrerFachRatio[ratioIndex]<1.0
		println("FAIL: "*string(lehrerStundenProFach[ratioIndex])*" teacher hours < "*string(faecherStunden[ratioIndex])*" hours for subject "*faecherNamen[ratioIndex]*"")
		println("###USERMSG###String138 "*faecherNamen[ratioIndex])
	else
		println("OK: "*string(lehrerStundenProFach[ratioIndex])*" teacher hours >= "*string(faecherStunden[ratioIndex])*" hours for subject "*faecherNamen[ratioIndex]*"")
	end
end

for lehrerIndex in LinearIndices(lehrerNamen)
	if lehrerMaxStunden[lehrerIndex]<lehrerStundenBelegt[lehrerIndex]
		println("FAIL: "*string(lehrerMaxStunden[lehrerIndex])*" max teacher hours < "*string(lehrerStundenBelegt[lehrerIndex])*" preallocated hours for teacher "*lehrerNamen[lehrerIndex]*"")
	end
	for courseIndex in LinearIndices(faecherNamen)
		if lehrerFachStundenBelegtSumme[lehrerIndex,courseIndex]>lehrerMaxStunden[lehrerIndex]
			println("FAIL: "*string(lehrerMaxStunden[lehrerIndex])*" max teacher hours < "*string(lehrerFachStundenBelegtSumme[lehrerIndex,courseIndex])*" target teacher hours for teacher "*lehrerNamen[lehrerIndex]*" im Fach "*faecherNamen[courseIndex]*"")
		end
	end
end

permutedKlassenIndices=collect(1:anzahlKlassen)
if classesInRandomOrder>0
	permutedKlassenIndices=randperm(MersenneTwister(classesInRandomOrder),anzahlKlassen)
end
println("###PROGRESS###",progress+=1)
println("Current progress: ",progress)

deadEnd=false
while ! deadEnd
	if isfile("abort")
		println("processing aborted.")
		deadEnd = true
		break
	end

	global deadEnd
	global backtraceMarker
	global maxRosterPrefillCount

	resetAllHelpers()

	global startTime=now()
	global (klassenIndex,tagIndex,stundenIndex)=(1,1,1)
	global globalCount=0
	global globalBacktraceCount=0
	global doBacktrace=false

	global allRostersPrefill
	global rosterPrefillCount=1
	global nextRosterDiff=1

	global faecherStundenLehrerIndex=1
	
	global progress
	global progressMax = maxRosterPrefillCount*length(faecherStundenLehrer)
	
	global rosterHash=Dict{UInt,UInt}()
	
	while rosterPrefillCount<=maxRosterPrefillCount && ! deadEnd
		if isfile("abort")
			println("processing aborted.")
			deadEnd = true
			break
		end
		
		empty!(rosterHash)
		
		now()
		while faecherStundenLehrerIndex<=length(faecherStundenLehrer) && ! deadEnd

			println("###PROGRESS###",progress + floor(Int, (100.0-progress) * rosterPrefillCount * faecherStundenLehrerIndex / progressMax ) - 5)

			if isfile("abort")
				println("processing aborted.")
				deadEnd = true
				break
			end
			
			global faecherStundenLehrerIndex
			global backtraceMarker
			faecherStundenLehrerEntry=faecherStundenLehrer[faecherStundenLehrerIndex]

			course=Int(faecherStundenLehrerEntry[1])

			resetHelpersNewCourse()

			lehrerCourseRatio=fill(zeros(Float64,2),anzahlLehrer)
			for lehrerIndex in 1:anzahlLehrer
				lehrerCourseRatio[lehrerIndex]=[lehrerIndex,lehrerFachStunden[lehrerIndex,course]/(1 + sum(lehrerFachStunden[lehrerIndex,:])-lehrerFachStunden[lehrerIndex,course])]
			end
			sort!(lehrerCourseRatio,by=x->x[2],rev=false)
			lehrerToUse=Array{Int,1}()
			for lehrerCourseRatioEntry in lehrerCourseRatio
				if lehrerCourseRatioEntry[2]>0 && ! issubset([Int(lehrerCourseRatioEntry[1])],lehrerNNindex)
					append!(lehrerToUse,Int(lehrerCourseRatioEntry[1]))
				end
			end
			for lehrerCourseRatioEntry in lehrerCourseRatio
				if lehrerCourseRatioEntry[2]>0 && issubset([Int(lehrerCourseRatioEntry[1])],lehrerNNindex)
					append!(lehrerToUse,Int(lehrerCourseRatioEntry[1]))
				end
			end
			lehrerToUsePerClass=fill(zeros(Int,length(lehrerToUse)),anzahlKlassen)
			lehrerToUsePerClass[1]=deepcopy(lehrerToUse)
			for i in 2:anzahlKlassen
				if length(lehrerToUsePerClass[i-1])>0
					lehrerToUsePerClass[i]=deepcopy(lehrerToUsePerClass[i-1][vcat(collect(2:length(lehrerToUse)),1)])
				else
					lehrerToUsePerClass[i]=deepcopy(lehrerToUsePerClass[i-1])
				end
			end

			if haskey(allBacktraceMarkerCourse,course)
				backtraceMarker=deepcopy(allBacktraceMarkerCourse[course])
			end

			println("Next course: ",course," ",faecherNamen[course])
			(klassenIndex,tagIndex,stundenIndex)=fillRoster(permutedKlassenIndices,course,lehrerToUsePerClass,tage,stunden,doBacktrace)

			if haskey(rosterHash,hash(rosterPrefill))
				deadEnd=true
			end

			if (klassenIndex,tagIndex,stundenIndex)==(-1,-1,-1)
				if faecherStundenLehrerIndex==1
					deadEnd=true
				else
					faecherStundenLehrerIndex-=1
					if haskey(allBacktraceMarkerCourse,course)
						delete!(allBacktraceMarkerCourse,course)
					end
			   end
			else
				rosterHash[hash(rosterPrefill)]=1
				allBacktraceMarkerCourse[course]=deepcopy(backtraceMarker)
				faecherStundenLehrerIndex+=1
			end
		end
		now()
		
		println("All courses allocated")
		
		if faecherStundenLehrerIndex>length(faecherStundenLehrer)
			faecherStundenLehrerIndex-=1
		end
		
		doBacktrace=true

		#deadEnd=true
		#break
		
		if ! deadEnd
			somethingChanged=true
			while somethingChanged
				somethingChanged=false

				klassenIndex=1
				while klassenIndex <= anzahlKlassen && klassenIndex>0

					if isfile("abort")
						println("processing aborted.")
						deadEnd = true
						break
					end

					global tageMitKapazitaet
					global klassenIndex

					rosterKlasseBelegung=[ [ rosterPrefill[stunden[stundenIndexTmp],tage[tagIndexTmp],klassenIndex]!=[0,0] for stundenIndexTmp in 1:length(stunden)] for tagIndexTmp in 1:length(tage)]
					firstStunde=findfirst.(rosterKlasseBelegung)
					lastStunde=findlast.(rosterKlasseBelegung)
					sumStunde=sum.(rosterKlasseBelegung)
					diffStunden=firstStunde+lastStunde
					breakout=false

					tageOverloaded=Array{Int,1}()
					tageMitKapazitaet=Array{Int,1}()
					if maximum(diffStunden[.! isnothing.(diffStunden)])-minimum(diffStunden[.! isnothing.(diffStunden)])>=2
						tageOverloaded=LinearIndices(sumStunde)[sumStunde.==maximum(sumStunde)]
					else
						tageWithFree=((lastStunde-firstStunde).+1)-sumStunde.>0
						tageMitKapazitaet=tage[tageWithFree]
						if length(tageMitKapazitaet)>0
							tageOverloaded=LinearIndices(sumStunde)[sumStunde.==maximum(sumStunde)]
						end
					end
					if length(tageOverloaded)>0
						for tagOverloadedIndex in tageOverloaded
							stundeToFlatIndex=findlast(rosterKlasseBelegung[tagOverloadedIndex])
							if  !isPreset(klassenIndex,tagOverloadedIndex,stundeToFlatIndex)
								if length(tageMitKapazitaet)==0
									tageMitKapazitaet=LinearIndices(sumStunde)[sumStunde.==minimum(sumStunde)]
								end
								for tagFreeIndex in tageMitKapazitaet
									if length(LinearIndices(rosterKlasseBelegung[tagFreeIndex])[rosterKlasseBelegung[tagFreeIndex]]) > 0
										localRosterKlasseBelegung=[ [ rosterPrefill[stunden[stundenIndexTmp],tage[tagIndexTmp],klassenIndex]!=[0,0] for stundenIndexTmp in 1:length(stunden)] for tagIndexTmp in 1:length(tage)]
										lastStundeFreeIndex=maximum(LinearIndices(localRosterKlasseBelegung[tagFreeIndex])[localRosterKlasseBelegung[tagFreeIndex]])
										for stundeFreeIndex in 1:lastStundeFreeIndex
											if ! localRosterKlasseBelegung[tagFreeIndex][stundeFreeIndex]
												for stundenIndexMid in 1:length(stunden)
													for tagIndexMid in 1:length(tage)
														if  localRosterKlasseBelegung[tagIndexMid][stundenIndexMid] &&
															(stundenIndexMid,tagIndexMid)!=(stundeToFlatIndex,tagOverloadedIndex)
																courseToFlat=rosterPrefill[stunden[stundeToFlatIndex],tage[tagOverloadedIndex],klassenIndex][1]
																lehrerToFlat=rosterPrefill[stunden[stundeToFlatIndex],tage[tagOverloadedIndex],klassenIndex][2]
																rosterPrefill[stunden[stundeToFlatIndex],tage[tagOverloadedIndex],klassenIndex]=[0,0]
																resetSpecificHelper(klassenIndex,lehrerToFlat,courseToFlat,tagOverloadedIndex,stundeToFlatIndex)
																courseToMove=rosterPrefill[stunden[stundenIndexMid],tage[tagIndexMid],klassenIndex][1]
																lehrerToMove=rosterPrefill[stunden[stundenIndexMid],tage[tagIndexMid],klassenIndex][2]
																rosterPrefill[stunden[stundenIndexMid],tage[tagIndexMid],klassenIndex]=[0,0]
																resetSpecificHelper(klassenIndex,lehrerToMove,courseToMove,tagIndexMid,stundenIndexMid)
																moved=0
																if  checkCourseAvailable(klassenIndex,courseToMove,tagFreeIndex,stundeFreeIndex) &&
																	( lehrerToMove==0 || checkTeacherAvailable(lehrerToMove,courseToMove,tagFreeIndex,stundeFreeIndex) ) &&
																	sum([ rosterPrefill[stunden[tmpStundenIndex],tage[tagFreeIndex],klassenIndex]==[courseToMove,lehrerToMove] for tmpStundenIndex in 1:length(stunden) ])==0 &&
																	!isPreset(klassenIndex,tagIndexMid,stundenIndexMid)
																		rosterPrefill[stunden[stundeFreeIndex],tage[tagFreeIndex],klassenIndex]=[courseToMove,lehrerToMove]
																		setSpecificHelper(klassenIndex,lehrerToMove,courseToMove,tagFreeIndex,stundeFreeIndex)
																		moved=1
																		if  checkCourseAvailable(klassenIndex,courseToFlat,tagIndexMid,stundenIndexMid) &&
																			( lehrerToFlat==0 || checkTeacherAvailable(lehrerToFlat,courseToFlat,tagIndexMid,stundenIndexMid) ) &&
																			sum([ rosterPrefill[stunden[tmpStundenIndex],tage[tagIndexMid],klassenIndex]==[courseToFlat,lehrerToFlat] for tmpStundenIndex in 1:length(stunden) ])==0
																				rosterPrefill[stunden[stundenIndexMid],tage[tagIndexMid],klassenIndex]=[courseToFlat,lehrerToFlat]
																				setSpecificHelper(klassenIndex,lehrerToFlat,courseToFlat,tagIndexMid,stundenIndexMid)
																				moved=2
																		end
																end
																if moved==1
																	rosterPrefill[stunden[stundeFreeIndex],tage[tagFreeIndex],klassenIndex]=[0,0]
																	resetSpecificHelper(klassenIndex,lehrerToMove,courseToMove,tagFreeIndex,stundeFreeIndex)
																end
																if moved<2
																	rosterPrefill[stunden[stundeToFlatIndex],tage[tagOverloadedIndex],klassenIndex]=[courseToFlat,lehrerToFlat]
																	setSpecificHelper(klassenIndex,lehrerToFlat,courseToFlat,tagOverloadedIndex,stundeToFlatIndex)
																	rosterPrefill[stunden[stundenIndexMid],tage[tagIndexMid],klassenIndex]=[courseToMove,lehrerToMove]
																	setSpecificHelper(klassenIndex,lehrerToMove,courseToMove,tagIndexMid,stundenIndexMid)
																end
																if moved==2
																	btHoldIndices=[ allBacktraceMarkerCourse[courseToFlat][btIndex]!=(klassenIndex,tagOverloadedIndex,stundeToFlatIndex) for btIndex in 1:length(allBacktraceMarkerCourse[courseToFlat]) ]
																	allBacktraceMarkerCourse[courseToFlat]=allBacktraceMarkerCourse[courseToFlat][btHoldIndices]
																	btHoldIndices=[ allBacktraceMarkerCourse[courseToMove][btIndex]!=(klassenIndex,tagIndexMid,stundenIndexMid) for btIndex in 1:length(allBacktraceMarkerCourse[courseToMove]) ]
																	allBacktraceMarkerCourse[courseToMove]=allBacktraceMarkerCourse[courseToMove][btHoldIndices]
																	somethingChanged=true
																	breakout=true
																end
														end
														breakout ? break : nothing
													end
													breakout ? break : nothing
												end
											end
											breakout ? break : nothing
										end
									end
									breakout ? break : nothing
								end
							end
							breakout ? break : nothing
						end
					end
					breakout ? nothing : klassenIndex+=1
				end

				klassenIndex=1
				while klassenIndex <= anzahlKlassen && klassenIndex>0

					if isfile("abort")
						println("processing aborted.")
						deadEnd = true
						break
					end

					global tageMitKapazitaet
					global klassenIndex

					rosterKlasseBelegung=[ [ rosterPrefill[stunden[stundenIndexTmp],tage[tagIndexTmp],klassenIndex]!=[0,0] for stundenIndexTmp in 1:length(stunden)] for tagIndexTmp in 1:length(tage)]
					firstStunde=findfirst.(rosterKlasseBelegung)
					lastStunde=findlast.(rosterKlasseBelegung)
					sumStunde=sum.(rosterKlasseBelegung)
					diffStunden=firstStunde+lastStunde
					breakout=false

					tageOverloaded=Array{Int,1}()
					tageMitKapazitaet=Array{Int,1}()
					if maximum(diffStunden[.! isnothing.(diffStunden)])-minimum(diffStunden[.! isnothing.(diffStunden)])>=2
						tageOverloaded=LinearIndices(sumStunde)[sumStunde.==maximum(sumStunde)]
					end
					if length(tageOverloaded)>0
						for tagOverloadedIndex in tageOverloaded
							stundeToFlatIndex=findlast(rosterKlasseBelegung[tagOverloadedIndex])
							if  !isPreset(klassenIndex,tagOverloadedIndex,stundeToFlatIndex)
								if length(tageMitKapazitaet)==0
									tageMitKapazitaet=LinearIndices(sumStunde)[sumStunde.==minimum(sumStunde)]
								end
								for tagFreeIndex in tageMitKapazitaet
									if length(LinearIndices(rosterKlasseBelegung[tagFreeIndex])[rosterKlasseBelegung[tagFreeIndex]]) > 0
										tagBeforePermutation=Dict{Int,Array{Array{Int,1},1}}()
										tagBeforePermutation[klassenIndex]=rosterPrefill[:,tagFreeIndex,klassenIndex]
										tagPermutationPossible=true
										while !breakout && tagPermutationPossible
											if isfile("abort")
												println("processing aborted.")
												breakout = true
												break
											end
											localRosterKlasseBelegung=[ [ rosterPrefill[stunden[stundenIndexTmp],tage[tagIndexTmp],klassenIndex]!=[0,0] for stundenIndexTmp in 1:length(stunden)] for tagIndexTmp in 1:length(tage)]
											lastStundeFreeIndex=maximum(LinearIndices(localRosterKlasseBelegung[tagFreeIndex])[localRosterKlasseBelegung[tagFreeIndex]])
											for stundeFreeIndex in 1:(stundeToFlatIndex-1)
												if ! localRosterKlasseBelegung[tagFreeIndex][stundeFreeIndex]
													allConnectedMoved=true
													courseToFlat=rosterPrefill[stunden[stundeToFlatIndex],tage[tagOverloadedIndex],klassenIndex][1]
													lehrerToFlat=rosterPrefill[stunden[stundeToFlatIndex],tage[tagOverloadedIndex],klassenIndex][2]
													rosterPrefill[stunden[stundeToFlatIndex],tage[tagOverloadedIndex],klassenIndex]=[0,0]
													resetSpecificHelper(klassenIndex,lehrerToFlat,courseToFlat,tagOverloadedIndex,stundeToFlatIndex)
													if  checkCourseAvailable(klassenIndex,courseToFlat,tagFreeIndex,stundeFreeIndex) &&
														( lehrerToFlat==0 || checkTeacherAvailable(lehrerToFlat,courseToFlat,tagFreeIndex,stundeFreeIndex) ) &&
														sum([ rosterPrefill[stunden[tmpStundenIndex],tage[tagFreeIndex],klassenIndex]==[courseToFlat,lehrerToFlat] for tmpStundenIndex in 1:length(stunden) ])==0
															allConnectedBackup=Dict{Int,Array{Int,1}}()
															if klassenFachKonnektor[klassenIndex,courseToFlat]>0
																konnekt=klassenFachKonnektor[klassenIndex,courseToFlat]
																konnektList=klassenFachKonnektorLookup[konnekt]
																for klasseFach in konnektList
																	connectedKlassenIndex=klasseFach[1]
																	if 	connectedKlassenIndex!=klassenIndex &&
																		rosterPrefill[stunden[stundeToFlatIndex],tage[tagOverloadedIndex],connectedKlassenIndex]==[courseToFlat,lehrerToFlat]
																			rosterPrefill[stunden[stundeToFlatIndex],tage[tagOverloadedIndex],connectedKlassenIndex]=[0,0]
																			resetSpecificHelper(connectedKlassenIndex,lehrerToFlat,courseToFlat,tagOverloadedIndex,stundeToFlatIndex)
																			if  rosterPrefill[stunden[stundeFreeIndex],tage[tagFreeIndex],connectedKlassenIndex]==[0,0] &&
																				checkCourseAvailable(connectedKlassenIndex,courseToFlat,tagFreeIndex,stundeFreeIndex) &&
																				( lehrerToFlat==0 || checkTeacherAvailable(lehrerToFlat,courseToFlat,tagFreeIndex,stundeFreeIndex) ) &&
																				sum([ rosterPrefill[stunden[tmpStundenIndex],tage[tagFreeIndex],connectedKlassenIndex]==[courseToFlat,lehrerToFlat] for tmpStundenIndex in 1:length(stunden) ])==0
																					allConnectedBackup[connectedKlassenIndex]=rosterPrefill[stundeFreeIndex,tagFreeIndex,connectedKlassenIndex]
																					rosterPrefill[stunden[stundeFreeIndex],tage[tagFreeIndex],connectedKlassenIndex]=[courseToFlat,lehrerToFlat]
																					setSpecificHelper(connectedKlassenIndex,lehrerToFlat,courseToFlat,tagFreeIndex,stundeFreeIndex)
																					
																			else
																				allConnectedMoved=false
																				break
																			end
																	end
																end
															end
															if !allConnectedMoved
																for connectedKlassenIndex in keys(allConnectedBackup)
																	rosterPrefill[stunden[stundeFreeIndex],tage[tagFreeIndex],connectedKlassenIndex]=[0,0]
																	resetSpecificHelper(connectedKlassenIndex,lehrerToFlat,courseToFlat,tagFreeIndex,stundeFreeIndex)
																	rosterPrefill[stunden[stundeToFlatIndex],tage[tagOverloadedIndex],connectedKlassenIndex]=[courseToFlat,lehrerToFlat]
																	setSpecificHelper(connectedKlassenIndex,lehrerToFlat,courseToFlat,tagOverloadedIndex,stundeToFlatIndex)
																end
															end
															if allConnectedMoved
																rosterPrefill[stunden[stundeFreeIndex],tage[tagFreeIndex],klassenIndex]=[courseToFlat,lehrerToFlat]
																setSpecificHelper(klassenIndex,lehrerToFlat,courseToFlat,tagFreeIndex,stundeFreeIndex)
																somethingChanged=true
																breakout=true
															end
													else
														allConnectedMoved=false
													end
													if !allConnectedMoved
														rosterPrefill[stunden[stundeToFlatIndex],tage[tagOverloadedIndex],klassenIndex]=[courseToFlat,lehrerToFlat]
														setSpecificHelper(klassenIndex,lehrerToFlat,courseToFlat,tagOverloadedIndex,stundeToFlatIndex)
													end
												end
												breakout ? break : nothing
											end
											if !breakout
												tagPermutationPossible=false
												(tagPermutationPossible,newFreeStunde)=PermuteDay(stundeToFlatIndex,tagFreeIndex,klassenIndex)
												if tagPermutationPossible
													courseToFlat=rosterPrefill[stundeToFlatIndex,tagOverloadedIndex,klassenIndex][1]
													if klassenFachKonnektor[klassenIndex,courseToFlat]>0
														connectedTagPermutationPossible=true
														konnekt=klassenFachKonnektor[klassenIndex,courseToFlat]
														konnektList=klassenFachKonnektorLookup[konnekt]
														for klasseFach in konnektList
															connectedKlassenIndex=klasseFach[1]
															if connectedKlassenIndex!=klassenIndex
																if !haskey(tagBeforePermutation,connectedKlassenIndex)
																	tagBeforePermutation[connectedKlassenIndex]=rosterPrefill[:,tagFreeIndex,connectedKlassenIndex]
																end
																connectedTagPermutationPossible=true
																while connectedTagPermutationPossible && rosterPrefill[newFreeStunde,tagFreeIndex,connectedKlassenIndex]!=[0,0]
																	(connectedTagPermutationPossible,_)=PermuteDay(stundeToFlatIndex,tagFreeIndex,connectedKlassenIndex)
																end
																if !connectedTagPermutationPossible
																	break
																end
															end
														end
														if !connectedTagPermutationPossible
															tagPermutationPossible=false
														end
													end
												end
											end
											#tagPermutationPossible=false
										end
										if !tagPermutationPossible
											#rollback day permutation
											for permutedKlassenIndex in keys(tagBeforePermutation)
												for stundenIndex in stunden
													course=rosterPrefill[stundenIndex,tagFreeIndex,permutedKlassenIndex][1]
													teacher=rosterPrefill[stundenIndex,tagFreeIndex,permutedKlassenIndex][2]
													oldCourse=tagBeforePermutation[permutedKlassenIndex][stundenIndex][1]
													oldTeacher=tagBeforePermutation[permutedKlassenIndex][stundenIndex][2]
													if course != oldCourse || teacher != oldTeacher
														rosterPrefill[stundenIndex,tagFreeIndex,permutedKlassenIndex]=[0,0]
														resetSpecificHelper(permutedKlassenIndex,teacher,course,tagFreeIndex,stundenIndex)
														rosterPrefill[stundenIndex,tagFreeIndex,permutedKlassenIndex]=[oldCourse,oldTeacher]
														setSpecificHelper(permutedKlassenIndex,oldTeacher,oldCourse,tagFreeIndex,stundenIndex)
													end
												end
											end
										end
									end
									breakout ? break : nothing
								end
							end
							breakout ? break : nothing
						end
					end
					breakout ? nothing : klassenIndex+=1
				end
			end
			
			diffCount=0
			if rosterPrefillCount>1
				for klassenIndex in 1:anzahlKlassen
					for tagIndex in 1:length(tage)
						for stundenIndex in 1:length(stunden)
							if allRostersPrefill[nextRosterDiff][stundenIndex,tagIndex,klassenIndex]!=rosterPrefill[stundenIndex,tagIndex,klassenIndex]
								diffCount+=1
							end
						end
					end
				end
			end
			
			if rosterPrefillCount==1 || diffCount>5
				println("Roster: ",rosterPrefillCount)
				println("Number of differences: ",diffCount)
				nextRosterDiff=rosterPrefillCount
				allRostersPrefill[rosterPrefillCount]=deepcopy(rosterPrefill)
				rosterPrefillCount+=1
			end
		end
	end
	if deadEnd
		println("Dead end encountered")
	end
	deadEnd = true
end

println("###PROGRESS###",98)

if length(keys(allRostersPrefill))>0
	println("Saving rosters")
else
	println("###USERMSG###String151")
end

function replaceSpecialChars(in::String)
	replace(replace(in,";" => "###SEMICOLON###"),":" => "###COLON###")
end

for rosterPrefillIndex in sort(collect(keys(allRostersPrefill)))
	outStream = open("timetable."*string(rosterPrefillIndex)*".csv","w")
	print(outStream,"\xEF\xBB\xBF")  #UTF-8 BOM header
	for klasseIndex in 1:length(klassenNamen)
		println(outStream,"###KLASSE###;")
		println(outStream,replaceSpecialChars(klassenNamen[klasseIndex])*";")
		print(outStream,";")
		for tagIndex in LinearIndices(tageNamen)
			print(outStream,replaceSpecialChars(tageNamen[tagIndex])*";")
		end
		println(outStream,"")
		for stundenIndex in LinearIndices(stundenNamen)
			print(outStream,replaceSpecialChars(stundenNamen[stundenIndex])*";")
			for tagIndex in LinearIndices(tageNamen)
				fachIndex=allRostersPrefill[rosterPrefillIndex][stundenIndex,tagIndex,klasseIndex][1]
				lehrerIndex=allRostersPrefill[rosterPrefillIndex][stundenIndex,tagIndex,klasseIndex][2]
				if fachIndex > 0
					fach=faecherNamen[fachIndex]
					print(outStream,replaceSpecialChars(fach))
				end
				if lehrerIndex > 0
					lehrerOut=lehrerNamen[lehrerIndex]
					print(outStream,":"*replaceSpecialChars(lehrerOut))
				end
				print(outStream,";")
			end
			println(outStream,"")
		end
	end
	close(outStream)
end

println("###PROGRESS###",100)



println("Finished")






















