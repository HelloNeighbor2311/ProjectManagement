$(function () {
    var l = abp.localization.getResource('ProjectManagement');

    var dataTable = $('#ProjectTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            processing: true,
            ajax: abp.libs.datatables.createAjax(projectManagement.projects.project.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                action: function (data) {
                                    editProject(data.record.id);
                                }
                            },
                            {
                                text: l('Delete'),
                                confirmMessage: function (data) {
                                    return l('DeleteConfirmationMessage');
                                },
                                action: function (data) {
                                    projectManagement.projects.project
                                        .delete(data.record.id)
                                        .then(function () {
                                            abp.notify.success(l('DeletedSuccessfully'));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                    }
                },
                {
                    title: l('Color'),
                    data: "color",
                    width: "10%",
                    render: function (data) {
                        return '<span style="display: inline-block; width: 20px; height: 20px; background-color: ' + (data || '#808080') + '; border-radius: 10px; border: 1px solid #ddd;"></span>';
                    },
                    defaultContent: ""
                },
                {
                    title: l('Name'),
                    data: "name",
                    width: "30%",
                    defaultContent: ""
                },
                {
                    title: l('Description'),
                    data: "description",
                    width: "50%",
                    render: function (data) {
                        return data || '-';
                    },
                    defaultContent: ""
                }
                
            ]
        })
    );

    function editProject(id) {
        abp.message.info(l('EditingProjectInfo') + ': ' + id);
        // TODO: Implement edit functionality
    }
});
