/*
---------------------------------------------
Copyright, Dr. Saeid Hosseini

https://sites.google.com/view/dr-seyed-saeid-hosseini/home
https://scholar.google.com/citations?hl=en&user=Q3DhDl0AAAAJ
http://people.sutd.edu.sg/~hosseini_saeid/

---------------------------------------------
Reference: Cite any of relevant articles as follows:

--Hosseini, Saeid, et al. "Leveraging multi-aspect time-related influence in location recommendation." World Wide Web (2017): 1-28.
--Hosseini, Saeid. "Location Inference and Recommendation in Social Networks." (2017).

--Hosseini, Saeid, and Lei Thor Li. "Point-of-interest recommendation using temporal orientations of users and locations." International Conference on Database Systems for Advanced Applications. Springer, Cham, 2016.
--Hosseini, Saeid, et al. "Jointly modeling heterogeneous temporal properties in location recommendation." International Conference on Database Systems for Advanced Applications. Springer, Cham, 2017.
---------------------------------------------
*/
/*
This file includes three stored procedures to implement User-based Collaborative Filtering CF methods in the recommendation.
Also, the mixture model which can include spatiotemporal effects into the User-based CF
*/
ALTER PROCEDURE [dbo].[Brightkite_Userbased_CF_30Percent]
		@userNum int		
AS
BEGIN

declare @Sum_ik float

select @Sum_ik=sum(s.w_ik) from Brightkite_TestUsers_Similarity_30_Percent s
where s.user_i=@userNum

;with UsersSharedPOIs as
(
select a.[user] as u_i, b.[user] as u_k, count(*) as SharedPOIs from Brightkite_TestUsers_Input a
inner join Brightkite_TestUsers_Input b on a.[user] != b.[user] and a.location_id=b.location_id
where (a.marked_off_30percent is null or a.marked_off_30percent=0) and (b.marked_off_30percent is null or b.marked_off_30percent=0)
group by a.[user],b.[user]
)
, distinctUsersOfSharedPOIs as
(
 SELECT u
FROM (
  SELECT u_i
  FROM UsersSharedPOIs
  UNION
  SELECT u_k
  FROM UsersSharedPOIs
) AS DistinctCodes (u)
WHERE u IS NOT NULL
) --select * from distinctUsersOfSharedPOIs
--select top 100 * from POIsOfdistinctUsersOfSharedPOIs
,UserKsLocations as
(
select u_i,u_k,uk.location_id,case when locationCount is null then 0 else 1 end as c_kj from UsersSharedPOIs us
left join Brightkite_TestUsers_Input uk on uk.[user]=us.u_k
where u_i=@userNum and (uk.marked_off_30percent is null or uk.marked_off_30percent=0)
)
--select * from UserKsLocations
,FullLoopUserLocations as
(
select u_i,u_k,uk.location_id,c_kj,s.w_ik from UserKsLocations uk
left join Brightkite_TestUsers_Similarity_30_Percent s on s.user_i=@userNum and s.user_k=uk.u_k
where location_id not in (select location_id from Brightkite_TestUsers_Input where [user]=@userNum and (marked_off_30percent is null or marked_off_30percent=0))
)
,Final_CF as
(
select @userNum as [user],location_id,sum(w_ik*c_kj)/@Sum_ik as cp_ij
from FullLoopUserLocations
group by location_id
having SUM(w_ik) is not null and SUM(w_ik)<>0
)
--insert into Brightkite_UserBased_CF_OutPut ([user],location_id,cp_ij,MarkedLOff_Rate)
--select [user],location_id,cp_ij from Final_CF order by location_id
insert into Brightkite_Output_30_Prcnt ([user],location_id,cp_ij)
select [user],location_id,cp_ij from Final_CF order by location_id
/*
UPDATE
    Brightkite_TestUsers_Output
SET
    Brightkite_TestUsers_Output.cp_ij_30_Prcnt = Final_CF.cp_ij    
FROM
    Brightkite_TestUsers_Output
inner JOIN
    Final_CF
ON
    Brightkite_TestUsers_Output.[user] = Final_CF.[user] and Brightkite_TestUsers_Output.location_id = Final_CF.location_id
	*/
/* top K items with the highest probability.
,Final_CF as
(
select ROW_NUMBER() OVER(order by sum(w_ik*c_kj)/@Sum_ik desc) AS RowID,@userNum as [user],location_id,sum(w_ik*c_kj)/@Sum_ik as cp_ij,@MarkedOff_Percent as MarkedLOff_Rate
from FullLoopUserLocations
group by location_id
having SUM(w_ik) is not null and SUM(w_ik)<>0
)
insert into Brightkite_UserBased_CF_OutPut ([user],location_id,cp_ij,MarkedLOff_Rate)
select top 100 [user],location_id,cp_ij,MarkedLOff_Rate from Final_CF
order by RowID
*/
END
/*********************/
ALTER PROCEDURE [dbo].[Brightkite_Collaborative_MixtureModel_FMeasure]
	@userNum bigint,
	@atNum int,
    @alpha float,
	@beta float
AS
BEGIN	
	SET NOCOUNT ON;
	/*
	cp_ij_norm: User based
	cp_ij_friend_norm: Friend based
	prob_gi_norm: Geographical baseds
	*/
	;with OutPutByUser as --get top N recommended POIs per user.
	(
	SELECT [user],location_id,cp_ij_norm,cp_ij_friend_norm,prob_gi_norm,num,measure from
	(
	SELECT [user],location_id,cp_ij_norm,cp_ij_friend_norm,prob_gi_norm,
	(1-@alpha-@beta)*cp_ij_norm+@alpha * cp_ij_friend_norm+@beta * prob_gi_norm as measure,
	ROW_NUMBER() OVER(PARTITION BY [user] ORDER BY ((1-@alpha-@beta)*cp_ij_norm+@alpha * cp_ij_friend_norm+@beta * prob_gi_norm) desc) num
	FROM Brightkite_Output_30_Prcnt
	where [user]=@userNum
	) X
	WHERE num <= @atNum
	)	
	,InputOutputQuery as --Get the information on Marked-off items from recommended POIs.
	(
	select top(@atNum) o.[user],o.location_id,case when i.marked_off_30percent is not null and i.marked_off_30percent=1 then 1 else 0 end as marked_off_30percent
	from OutPutByUser o
	left join Brightkite_TestUsers_Input i on i.location_id=o.location_id and i.[user]=o.[user]
	group by o.[user],o.location_id,i.marked_off_30percent
	having o.[user]=@userNum
	)
	,
	RecallPerUser as --The rate of recovered POI to the set of POI deleted in preprocessing : Recall Rate Per user.
	(
	select i.[user],cast(sum(case when o.marked_off_30percent is not null and o.marked_off_30percent=1 then 1 else 0 end) as float)/case when cast(sum(case when i.marked_off_30percent is not null and i.marked_off_30percent=1 then 1 else 0 end) as float)=0 then 1 else cast(sum(case when i.marked_off_30percent is not null and i.marked_off_30percent=1 then 1 else 0 end) as float) end as RecallPerUser
	from Brightkite_TestUsers_Input i
	left join InputOutputQuery o on i.location_id=o.location_id
	group by i.[user]
	having i.[user]=@userNum
	)
	,PrecisionPerUser as --the ratio of recovered POIs to the N recommended POIs
	(
	select [user],cast(sum(case when marked_off_30percent is not null and marked_off_30percent=1 then 1 else 0 end) as float)/cast(count(*) as float) as PrecisionPerUser
	from InputOutputQuery
	group by [user]
	having [user]=@userNum
	)
	--INSERT INTO Brightkite_Fmeasure_Results ([user],N,alpha,beta,[precision],recall,fmeasure,markedoffpercent)
	select @userNum as userNum,@atNum,@alpha,@beta,PrecisionPerUser,RecallPerUser,2*PrecisionPerUser*RecallPerUser/case when (PrecisionPerUser+RecallPerUser)=0 then 1 else (PrecisionPerUser+RecallPerUser) end,30 from RecallPerUser,PrecisionPerUser
	END
/*****************/
ALTER PROCEDURE [dbo].[Brightkite_OneStep_UB_CF_Temporal_Optimized_Ver01_UserActOnly]
	@userNum bigint,
	@atNum int,
    @alpha float,
	@beta float
AS
BEGIN	
	SET NOCOUNT ON;

/*
Descritpion: The version of temporal
It considers how much the POI has been used by others. then if it hasn't been used before then it is excluded. 
*/

declare @Sum_ik float

--get the user's temporal Act
declare @UserAct_avgWE int,@UserAct_avgWD int

;with a0 as --Select Marked Off items. Study how they have been used during the weekend and weekday.
(
select lfc.[user],lfc.location_id,t.marked_off_30percent,case when datepart(dw,checkin_time) in (1,6,7) then 1 else 0 end as WeekEnd_Sign,case when datepart(dw,checkin_time) in (1,6,7) then 0 else 1 end as WeekDay_Sign,checkin_time
from Brightkite_totalCheckins lfc
inner join Brightkite_TestUsers_Input t on t.[user]=lfc.[user] and t.location_id=lfc.location_id
where lfc.[user]=@userNum and (marked_off_30percent is not null and marked_off_30percent=1) --make sure that the ratio of weekend and weekday is correct
)--select * from a0 order by VenueName
,b0 as --Now for marked off items calculate the [POI act] based on the current user's prefrence.
(
select [user],location_id,cast(sum(WeekEnd_Sign) as float)/cast(count(*) as float) POI_Act_Weekend,cast(sum(WeekDay_Sign) as float)/cast(count(*) as float) POI_Act_Weekday,count(*) as this_UserCount --,CountShared
from a0
group by [user],location_id--,CountShared
)
, b1 as --Cold start POIs which are not used by many users makes the alignment of the user inaccurate. So in order to compute the we consider those POIs that are used at least by another user.
--Infact this ruls is derivated from the weight and probability of the user to visit a location. In user-poi space if a POI hasn't been visited by more than 1 user, then it will not be essentially recommended.
(
select b0.[user],b0.location_id,b0.POI_Act_Weekend,b0.POI_Act_Weekday,COUNT(*) as CountShared,this_UserCount from b0
left join Brightkite_TestUsers_Input t on b0.[user]!=t.[user] and b0.location_id=t.location_id
where (t.marked_off_30percent is null or t.marked_off_30percent=0)
group by b0.[user],b0.location_id,b0.POI_Act_Weekday,b0.POI_Act_Weekend,this_UserCount
having COUNT(*)>1
) --select * from b1 --testing
, b2 as 
(
select b1.[user],b1.location_id,POI_Act_Weekend,POI_Act_Weekday,CountShared,o.cp_ij_norm from b1
left join Brightkite_Output_30_Prcnt o on o.[user]=b1.[user] and b1.location_id=o.location_id
)--select * from b2
,b2MaxMin as
(
select [user],max(cp_ij_norm) as MaxProb,min(cp_ij_norm) as  MinProb from b2
group by [user]
)
,b as
(
select b20.[user],((b20.cp_ij_norm-bmxmn.MinProb)/(bmxmn.MaxProb-bmxmn.MinProb))*(b20.POI_Act_Weekday-0.5) as POI_Act_Weekday_Normalized,((b20.cp_ij_norm-bmxmn.MinProb)/(bmxmn.MaxProb-bmxmn.MinProb))*(b20.POI_Act_Weekend-0.5) as POI_Act_Weekend_Normalized from b2 as b20
left join b2MaxMin as bmxmn on b20.[user]=bmxmn.[user]
)
,d as --Now calculate the user act based on markedoff-items
(
select [user],avg(POI_Act_Weekend_Normalized) as avgWE,avg(POI_Act_Weekday_Normalized) as avgWD
from b
group by [user]
)
select @UserAct_avgWE = round((avgWE+0.5)*@atNum,0),@UserAct_avgWD=round((avgWD+0.5)*@atNum,0) from d

print cast(@UserAct_avgWE as float)/cast((@UserAct_avgWE+@UserAct_avgWD) as float)
print cast(@UserAct_avgWD as float)/cast((@UserAct_avgWE+@UserAct_avgWD) as float)

print cast(@UserAct_avgWE as float)/cast((@UserAct_avgWE+@UserAct_avgWD) as float)-cast(@UserAct_avgWD as float)/cast((@UserAct_avgWE+@UserAct_avgWD) as float)



IF @atNum=5
BEGIN
    update Brightkite_test_users_30Percent set 
	    act_optimized_ver01_At5=cast(@UserAct_avgWE as float)/cast((@UserAct_avgWE+@UserAct_avgWD) as float)-cast(@UserAct_avgWD as float)/cast((@UserAct_avgWE+@UserAct_avgWD) as float)
    where [user]=@userNum
END

IF @atNum=10
BEGIN
    update Brightkite_test_users_30Percent set 
	    act_optimized_ver01_At10=cast(@UserAct_avgWE as float)/cast((@UserAct_avgWE+@UserAct_avgWD) as float)-cast(@UserAct_avgWD as float)/cast((@UserAct_avgWE+@UserAct_avgWD) as float)
    where [user]=@userNum
END

IF @atNum=20
BEGIN
    update Brightkite_test_users_30Percent set 
	    act_optimized_ver01_At20=cast(@UserAct_avgWE as float)/cast((@UserAct_avgWE+@UserAct_avgWD) as float)-cast(@UserAct_avgWD as float)/cast((@UserAct_avgWE+@UserAct_avgWD) as float)
    where [user]=@userNum
END

/*
alter table Brightkite_test_users_30Percent
add act_optimized_ver01_At20 float
*/
end
